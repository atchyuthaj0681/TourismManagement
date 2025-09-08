using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismManagement.Data;
using TourismManagement.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System;
using TourismManagement.Models;

namespace TourismManagement.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: BookNow - Booking Form
       

        // POST: BookNow - Booking submission
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BookNow(BookingFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailablePackages = _context.Packages.ToList();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var package = await _context.Packages.FindAsync(model.PackageId);
            if (package == null) return NotFound();

            var totalAmount = package.Price * model.NumPeople;

            var booking = new Booking
            {
                PackageId = model.PackageId,
                UserId = user.Id,
                TravelDate = model.TravelDate,
                NumPeople = model.NumPeople,
                BookingDate = DateTime.Now,
                TotalAmount = totalAmount,
                Status = "Booked"
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("BookingConfirmation");
        }
        [HttpGet]
        [Authorize]
        public IActionResult BookNow(int? packageId = null)
        {
            var model = new BookingFormViewModel
            {
                AvailablePackages = _context.Packages.ToList(),
                PackageId = packageId ?? 0 // Set selected package if passed
            };

            return View(model);
        }


        public IActionResult BookingConfirmation()
        {
            return View();
        }

        // Booking history for customers
        [Authorize]
        public async Task<IActionResult> BookingHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var bookings = await _context.Bookings
                .Include(b => b.Package)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var model = new BookingListViewModel
            {
                Bookings = bookings
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Package)
                .Include(b => b.User) // Optional: if you want to verify ownership
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("BookingHistory");
            }

            // Optional: Ensure user can only cancel their own booking
            var userEmail = User.Identity?.Name;
            if (booking.User.Email != userEmail)
            {
                TempData["Error"] = "Unauthorized action.";
                return RedirectToAction("BookingHistory");
            }

            // Check if already cancelled
            if (booking.Status == "Cancelled")
            {
                TempData["Info"] = "This booking has already been cancelled.";
                return RedirectToAction("BookingHistory");
            }

            // Check travel date
            if (booking.TravelDate <= DateTime.Now)
            {
                TempData["Error"] = "You cannot cancel past or ongoing bookings.";
                return RedirectToAction("BookingHistory");
            }

            // Proceed to cancel
            booking.Status = "Cancelled";
            booking.CancellationDate = DateTime.Now;

            // Apply 15% cancellation fee
            booking.RefundAmount = Math.Round(booking.TotalAmount * 0.85m, 2);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Booking for {booking.Package.Title} on {booking.TravelDate:MMM dd, yyyy} has been cancelled. Refund: {booking.RefundAmount:C}";

            return RedirectToAction("BookingHistory");
        }

        // GET: Booking/BookingDetails/5
        [Authorize]
        public async Task<IActionResult> BookingDetails(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Package)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (booking.UserId != currentUser.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(booking);
        }


        // GET: Booking/EditBooking/5
        [Authorize]
        public async Task<IActionResult> EditBooking(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var booking = await _context.Bookings
                .Include(b => b.Package)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

            if (booking == null)
                return NotFound();

            if (booking.Status == "Cancelled" || booking.TravelDate <= DateTime.Now)
            {
                TempData["Error"] = "You cannot edit a cancelled or past booking.";
                return RedirectToAction("BookingHistory");
            }

            var model = new EditBookingViewModel
            {
                Id = booking.Id,
                TravelDate = booking.TravelDate,
                NumPeople = booking.NumPeople,
                TotalAmount = booking.TotalAmount
            };

            return View(model);
        }

        // POST: Booking/EditBooking
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditBooking(EditBookingViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            var booking = await _context.Bookings
                .Include(b => b.Package)
                .FirstOrDefaultAsync(b => b.Id == model.Id && b.UserId == user.Id);

            if (booking == null)
                return NotFound();

            if (booking.Status == "Cancelled" || booking.TravelDate <= DateTime.Now)
            {
                TempData["Error"] = "You cannot edit a cancelled or past booking.";
                return RedirectToAction("BookingHistory");
            }

            // Update allowed fields
            booking.TravelDate = model.TravelDate;
            booking.NumPeople = model.NumPeople;

            // Recalculate total amount
            booking.TotalAmount = booking.Package.Price * model.NumPeople;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking updated successfully.";
            return RedirectToAction("BookingHistory");
        }


        // Admin: Manage all bookings with filters and sorting
        [Authorize(Roles = "Admin")]
        public IActionResult ManageBookings(string? status, string? userEmail, int? packageId, string sortOrder = "date_desc")
        {
            var query = _context.Bookings
                .Include(b => b.Package)
                .Include(b => b.User)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }

            // Filter by user email
            if (!string.IsNullOrEmpty(userEmail))
            {
                query = query.Where(b => b.User.Email.Contains(userEmail));
            }

            // Filter by package
            if (packageId.HasValue)
            {
                query = query.Where(b => b.PackageId == packageId.Value);
            }

            // Sorting
            query = sortOrder switch
            {
                "date_asc" => query.OrderBy(b => b.BookingDate),
                "amount_desc" => query.OrderByDescending(b => b.TotalAmount),
                "amount_asc" => query.OrderBy(b => b.TotalAmount),
                _ => query.OrderByDescending(b => b.BookingDate), // default date_desc
            };

            var bookings = query.ToList();

            var model = new BookingListViewModel
            {
                Bookings = bookings,
                Status = status,
                UserEmail = userEmail,
                PackageId = packageId,
                SortOrder = sortOrder,
                Packages = _context.Packages.ToList()
            };

            return View(model);
        }
    }
}
