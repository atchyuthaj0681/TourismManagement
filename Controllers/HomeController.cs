using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using TourismManagement.Data;
using TourismManagement.Models;

namespace TourismManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // List all packages - accessible to everyone
        [AllowAnonymous]
        public async Task<IActionResult> Packages()
        {
            var packages = await _context.Packages.ToListAsync();
            return View(packages);
        }

        // View package details by Id
        [AllowAnonymous]
        public async Task<IActionResult> PackageDetails(int? id)
        {
            if (id == null)
                return NotFound();

            var package = await _context.Packages.FirstOrDefaultAsync(p => p.Id == id);

            if (package == null)
                return NotFound();

            return View(package);
        }

        // Create package GET
        [Authorize(Roles = "Admin")] // or your preferred role
        public IActionResult CreatePackage()
        {
            return View();
        }

        // Create package POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePackage(Package package)
        {
            if (ModelState.IsValid)
            {
                _context.Packages.Add(package);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Packages));
            }
            return View(package);
        }

        // Edit package GET
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditPackage(int? id)
        {
            if (id == null)
                return NotFound();

            var package = await _context.Packages.FindAsync(id);

            if (package == null)
                return NotFound();

            return View(package);
        }

        // Edit package POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditPackage(int id, Package package)
        {
            if (id != package.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(package);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Packages.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Packages));
            }
            return View(package);
        }

        // Delete package GET - confirm delete
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePackage(int? id)
        {
            if (id == null)
                return NotFound();

            var package = await _context.Packages.FirstOrDefaultAsync(p => p.Id == id);

            if (package == null)
                return NotFound();

            return View(package);
        }

        // Delete package POST - actually delete
        [HttpPost, ActionName("DeletePackage")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePackageConfirmed(int id)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package != null)
            {
                _context.Packages.Remove(package);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Packages));
        }

        [Authorize]  // User must be logged in to book
        public async Task<IActionResult> BookNow(int? packageId)  // Added packageId parameter here
        {
            // Get active packages to show in dropdown
            var packages = await _context.Packages
                .Where(p => p.Status == "Active")
                .ToListAsync();

            ViewBag.Packages = packages;
            ViewBag.SelectedPackageId = packageId;  // Pass selected package id to the view

            return View();
        }

        // Make sure you have this at the top

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> BookNow(Booking booking)
        {
            if (!ModelState.IsValid)
            {
                Debug.WriteLine("ModelState invalid.");
                foreach (var e in ModelState.Values.SelectMany(v => v.Errors))
                    Debug.WriteLine($"Error: {e.ErrorMessage}");
                ViewBag.Packages = await _context.Packages.ToListAsync();
                return View(booking);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Debug.WriteLine($"User is {userId}");

            booking.UserId = userId;
            booking.BookingDate = DateTime.Now;

            var package = await _context.Packages.FindAsync(booking.PackageId);
            if (package == null)
            {
                Debug.WriteLine("Package not found");
                // ...
            }

            booking.TotalPrice = package.Price * booking.NumberOfPeople;
            booking.BookingStatus = "Confirmed";
            booking.PaymentStatus = "Pending";

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            Debug.WriteLine($"Booking saved with ID {booking.Id} and UserId {booking.UserId}");

            return RedirectToAction("BookingHistory");
        }


        

    public IActionResult BookingConfirmation()
        {
            return View();
        }

        public async Task<IActionResult> BookingHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Debug.WriteLine($"Retrieving for user {userId}");

            var bookings = await _context.Bookings
                .Include(b => b.Package)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            Debug.WriteLine($"Found {bookings.Count} bookings for this user");
            return View(bookings);
        }


        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bookings = await _context.Bookings
                                .Include(b => b.Package)
                                .Where(b => b.UserId == userId)
                                .ToListAsync();

            return View(bookings);
        }


        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Search(string query)
        {
            // TODO: Implement search logic here (search packages or destinations)
            ViewBag.Query = query;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            if (booking.UserId != userId)
                return Forbid();

            if (booking.BookingStatus == "Confirmed")
            {
                booking.BookingStatus = "Cancelled";
                booking.PaymentStatus = "Refund Initiated";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(BookingHistory));
        }
    }
}
