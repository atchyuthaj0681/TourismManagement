using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TourismManagement.Data;
using TourismManagement.Models.ViewModels;

namespace TourismManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
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
                                        .Select(p => p.Destination)
                                        .Distinct()
                                        .CountAsync();

            var totalPeople = await _context.Bookings.SumAsync(b => (int?)b.NumberOfPeople) ?? 0;

            // Booking counts grouped by destination
            var bookingCountByDestination = await _context.Bookings
                .Include(b => b.Package)
                .GroupBy(b => b.Package.Destination)
                .Select(g => new
                {
                    Destination = g.Key ?? "Unknown",
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
