using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TourismManagement.Data;
using TourismManagement.Models.ViewModels;

namespace TourismManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Total counts
            var totalUsers = await _context.Users.CountAsync();
            var totalBookings = await _context.Bookings.CountAsync();

            var totalDestinations = await _context.Packages
                                        .Where(p => !string.IsNullOrEmpty(p.Destination))
                                        .Select(p => p.Destination)
                                        .Distinct()
                                        .CountAsync();

            var totalPeople = await _context.Bookings
                                    .Where(b => b.Status != "Cancelled") // optional filter
                                    .SumAsync(b => (int?)b.NumPeople) ?? 0;

            // Booking counts grouped by destination
            var bookingCountByDestination = await _context.Bookings
                .Include(b => b.Package)
                .Where(b => b.Package != null)
                .GroupBy(b => b.Package.Destination ?? "Unknown")
                .Select(g => new
                {
                    Destination = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Destination, x => x.Count);

            // Prepare view model
            var model = new DashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalBookings = totalBookings,
                TotalDestinations = totalDestinations,
                TotalPeople = totalPeople,
                BookingCountByDestination = bookingCountByDestination
            };

            return View(model);
        }
    }
}
