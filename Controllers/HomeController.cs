using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TourismManagement.Data;
using TourismManagement.Models;
using TourismManagement.Models.ViewModels;

namespace TourismManagement.Controllers
{
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
            var packages = _context.Packages.ToList(); // This works because ApplicationDbContext has DbSet<Package>

            var viewModel = new PackageListViewModel
            {
                Packages = packages
            };

            return View(viewModel);
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

        // Admin-only: Create package
        [Authorize(Roles = "Admin")]
        public IActionResult CreatePackage()
        {
            return View();
        }

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

        // Admin-only: Edit package
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

        // Admin-only: Delete package
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

        // Static Pages
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
            ViewBag.Query = query;
            return View();
        }
    }
}
