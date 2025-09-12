using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            // Get the IDs of the top 3 most booked packages
            var trendingPackageIds = await _context.Bookings
                .Where(b => b.PackageId != null)
                .GroupBy(b => b.PackageId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToListAsync();

            // Get the packages that match those IDs
            var packages = await _context.Packages
                .Where(p => trendingPackageIds.Contains(p.Id) && p.Status == "Active")
                .ToListAsync();

            // If there are fewer than 3 trending packages, get some of the latest active packages to fill the gap
            if (packages.Count < 3)
            {
                var additionalPackages = await _context.Packages
                    .Where(p => p.Status == "Active" && !trendingPackageIds.Contains(p.Id))
                    .OrderByDescending(p => p.Id)
                    .Take(3 - packages.Count)
                    .ToListAsync();

                packages.AddRange(additionalPackages);
            }

            // Ensure unique packages and take a reasonable number for display on the home page
            packages = packages.DistinctBy(p => p.Id).Take(5).ToList(); // Limit to 5 for the scrollable list

            var viewModel = new PackageListViewModel
            {
                Packages = packages
            };

            return View(viewModel);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Packages()
        {
            var packages = await _context.Packages.ToListAsync();
            return View(packages);
        }

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

        [Authorize(Roles = "Admin")]
        public IActionResult CreatePackage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePackage(Package package, List<IFormFile> ImageFiles)
        {
            if (ModelState.IsValid)
            {
                List<string> savedImagePaths = new List<string>();

                if (ImageFiles != null && ImageFiles.Count > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    foreach (var image in ImageFiles)
                    {
                        if (image.Length > 0)
                        {
                            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }

                            savedImagePaths.Add("/uploads/" + uniqueFileName);
                        }
                    }
                }

                package.ImagePathString = string.Join(",", savedImagePaths);
                _context.Packages.Add(package);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Packages));
            }
            return View(package);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditPackage(int id)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package == null) return NotFound();

            return View(package);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPackage(int id, [Bind("Id,Title,Destination,DurationDays,Price,Description,Itinerary,Status,ImagePathString")] Package model, List<IFormFile> ImageFiles)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            var package = await _context.Packages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == model.Id);
            if (package == null) return NotFound();

            package.Title = model.Title;
            package.Destination = model.Destination;
            package.DurationDays = model.DurationDays;
            package.Price = model.Price;
            package.Description = model.Description;
            package.Itinerary = model.Itinerary;
            package.Status = model.Status;

            List<string> updatedImagePaths = new List<string>();
            if (!string.IsNullOrEmpty(model.ImagePathString))
            {
                updatedImagePaths.AddRange(model.ImagePathString.Split(',', StringSplitOptions.RemoveEmptyEntries));
            }

            if (ImageFiles != null && ImageFiles.Count > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                foreach (var imageFile in ImageFiles)
                {
                    if (imageFile.Length > 0)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        updatedImagePaths.Add("/uploads/" + uniqueFileName);
                    }
                }
            }

            package.ImagePathString = string.Join(",", updatedImagePaths);

            _context.Update(package);
            await _context.SaveChangesAsync();

            return RedirectToAction("PackageDetails", new { id = package.Id });
        }

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