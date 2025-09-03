using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
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

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Bookings
        public async Task<IActionResult> Index(string searchString, string paymentFilter, string bookingFilter, int pageNumber = 1)
        {
            int pageSize = 10;

            var bookings = _context.Bookings.Include(b => b.Package).AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b => b.Package.Title.Contains(searchString) || b.UserId.ToString().Contains(searchString));
            }

            // Filter Payment Status
            if (!string.IsNullOrEmpty(paymentFilter))
            {
                bookings = bookings.Where(b => b.PaymentStatus == paymentFilter);
            }

            // Filter Booking Status
            if (!string.IsNullOrEmpty(bookingFilter))
            {
                bookings = bookings.Where(b => b.BookingStatus == bookingFilter);
            }

            // Count total for pagination
            var count = await bookings.CountAsync();

            // Fetch page data
            var items = await bookings
                .OrderByDescending(b => b.BookingDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pass data and paging info to view
            var model = new BookingListViewModel
            {
                Bookings = items,
                SearchString = searchString,
                PaymentFilter = paymentFilter,
                BookingFilter = bookingFilter,
                Pagination = new PaginationModel
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalItems = count
                }
            };

            return View(model);
        }


        // GET: Admin/Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Package)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: Admin/Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Admin/Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        // GET: Admin/Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Package)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Admin/Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}
