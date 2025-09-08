using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using TourismManagement.Data;
using TourismManagement.Models;
using TourismManagement.Models.ViewModels;

namespace TourismManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Bookings
        public async Task<IActionResult> Index(string status, string userEmail, int? packageId, int page = 1, string sortOrder = "desc")
        {
            var bookingsQuery = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Package)
                .AsQueryable();

            // Filtering
            if (!string.IsNullOrWhiteSpace(status))
                bookingsQuery = bookingsQuery.Where(b => b.Status == status);

            if (!string.IsNullOrWhiteSpace(userEmail))
                bookingsQuery = bookingsQuery.Where(b => b.User.Email.Contains(userEmail));

            if (packageId.HasValue && packageId > 0)
                bookingsQuery = bookingsQuery.Where(b => b.PackageId == packageId.Value);

            // Sorting
            bookingsQuery = sortOrder.ToLower() switch
            {
                "asc" => bookingsQuery.OrderBy(b => b.BookingDate),
                "amount_asc" => bookingsQuery.OrderBy(b => b.TotalAmount),
                "amount_desc" => bookingsQuery.OrderByDescending(b => b.TotalAmount),
                _ => bookingsQuery.OrderByDescending(b => b.BookingDate), // default desc
            };

            // Pagination
            var totalItems = await bookingsQuery.CountAsync();
            var bookings = await bookingsQuery
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var viewModel = new BookingListViewModel
            {
                Bookings = bookings,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize),
                Status = status,
                UserEmail = userEmail,
                PackageId = packageId,
                SortOrder = sortOrder,
                Packages = await _context.Packages.ToListAsync()
            };

            return View(viewModel);
        }

        // POST: Admin/Bookings/CancelBooking/5
        [HttpPost]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null || booking.Status == "Cancelled")
            {
                TempData["Error"] = "Booking not found or already cancelled.";
                return RedirectToAction(nameof(Index));
            }

            if (booking.TravelDate <= DateTime.Now)
            {
                TempData["Error"] = "Cannot cancel past or ongoing bookings.";
                return RedirectToAction(nameof(Index));
            }

            booking.Status = "Cancelled";
            booking.CancellationDate = DateTime.Now;
            booking.RefundAmount = booking.TotalAmount - (booking.TotalAmount * 0.15m); // 15% penalty

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking cancelled successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Bookings/ExportCsv
        public async Task<IActionResult> ExportCsv(string status, string userEmail, int? packageId)
        {
            var bookingsQuery = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Package)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                bookingsQuery = bookingsQuery.Where(b => b.Status == status);

            if (!string.IsNullOrWhiteSpace(userEmail))
                bookingsQuery = bookingsQuery.Where(b => b.User.Email.Contains(userEmail));

            if (packageId.HasValue && packageId > 0)
                bookingsQuery = bookingsQuery.Where(b => b.PackageId == packageId.Value);

            var bookings = await bookingsQuery.ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("UserEmail,PackageTitle,TravelDate,NumPeople,TotalAmount,Status,CancellationDate,RefundAmount");

            foreach (var b in bookings)
            {
                csv.AppendLine($"\"{b.User.Email}\",\"{b.Package.Title}\",\"{b.TravelDate:yyyy-MM-dd}\",\"{b.NumPeople}\",\"{b.TotalAmount.ToString("F2", CultureInfo.InvariantCulture)}\",\"{b.Status}\",\"{(b.CancellationDate.HasValue ? b.CancellationDate.Value.ToString("yyyy-MM-dd") : "")}\",\"{(b.RefundAmount.HasValue ? b.RefundAmount.Value.ToString("F2", CultureInfo.InvariantCulture) : "")}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "BookingsReport.csv");
        }
    }
}
