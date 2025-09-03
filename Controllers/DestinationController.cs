using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismManagement.Data;
using TourismManagement.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TourismManagement.Controllers
{
    public class DestinationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DestinationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // List all destinations with search, filter, sort, pagination
        public async Task<IActionResult> Index(string search, string category, string sortOrder, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Destinations.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(d => d.Name.Contains(search) || d.Description.Contains(search));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(d => d.Category == category);

            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(d => d.Price),
                "price_desc" => query.OrderByDescending(d => d.Price),
                "name_desc" => query.OrderByDescending(d => d.Name),
                _ => query.OrderBy(d => d.Name)
            };

            var totalItems = await query.CountAsync();
            var destinations = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.SortOrder = sortOrder;

            return View(destinations);
        }

        // View details of a destination
        public async Task<IActionResult> Details(int id)
        {
            var destination = await _context.Destinations
                                            .Include(d => d.Images)  // Include related images
                                            .FirstOrDefaultAsync(d => d.Id == id);

            if (destination == null)
            {
                return NotFound();
            }

            return View(destination);
        }

        // Show create form (admin only)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // Handle create POST (admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Destination destination)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Destinations.Add(destination);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving destination: " + ex.Message);
                }
            }
            return View(destination);
        }

        // Show edit form (admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var destination = await _context.Destinations.FindAsync(id);
            if (destination == null)
                return NotFound();

            return View(destination);
        }

        // Handle edit POST (admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Destination destination)
        {
            if (id != destination.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(destination);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Destinations.Any(d => d.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating destination: " + ex.Message);
                }
            }
            return View(destination);
        }

        // Delete destination (admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var destination = await _context.Destinations.FindAsync(id);
            if (destination == null)
                return NotFound();

            try
            {
                _context.Destinations.Remove(destination);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting destination: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Upload images for destination (admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImages(int id, List<IFormFile> images)
        {
            var destination = await _context.Destinations.Include(d => d.Images).FirstOrDefaultAsync(d => d.Id == id);
            if (destination == null)
                return NotFound();

            if (images == null || images.Count == 0)
            {
                TempData["Error"] = "Please select images to upload.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var maxFileSize = 5 * 1024 * 1024; // 5 MB

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "destinations");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["Error"] = "Only image files (.jpg, .jpeg, .png, .gif) are allowed.";
                        return RedirectToAction(nameof(Edit), new { id });
                    }

                    if (image.Length > maxFileSize)
                    {
                        TempData["Error"] = "File size must be less than 5MB.";
                        return RedirectToAction(nameof(Edit), new { id });
                    }

                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadPath, fileName);

                    try
                    {
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await image.CopyToAsync(stream);

                        destination.Images.Add(new DestinationImage { ImageUrl = "/images/destinations/" + fileName });
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = "Error uploading image: " + ex.Message;
                        return RedirectToAction(nameof(Edit), new { id });
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Images uploaded successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error saving images: " + ex.Message;
            }

            return RedirectToAction(nameof(Edit), new { id });
        }

        // Rate destination (logged in users)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RateDestination(int id, int rating, string review)
        {
            var destination = await _context.Destinations.FindAsync(id);
            if (destination == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // TODO: Implement actual rating logic here

            TempData["Message"] = "Thank you for rating this destination!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Filter destinations by category
        public async Task<IActionResult> FilterByCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return RedirectToAction(nameof(Index));

            var destinations = await _context.Destinations.Where(d => d.Category == category).ToListAsync();

            ViewBag.Category = category;
            return View("Index", destinations);
        }

        // Search bar handling
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
                return RedirectToAction(nameof(Index));

            var results = await _context.Destinations
                .Where(d => d.Name.Contains(query) || d.Description.Contains(query))
                .ToListAsync();

            ViewBag.Search = query;
            return View("Index", results);
        }
    }
}
