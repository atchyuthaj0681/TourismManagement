// File: TourismManagement.Controllers.DashboardController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TourismManagement.Data;
using TourismManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using TourismManagement.Models;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;

[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Displays the dashboard page
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        string userName = currentUser?.Name ?? User.Identity.Name;

        var totalUsers = await _context.Users.CountAsync();
        var totalBookings = await _context.Bookings.CountAsync();
        var totalPackages = await _context.Packages.CountAsync();

        var trendingPackageIds = await _context.Bookings
            .Where(b => b.PackageId != null)
            .GroupBy(b => b.PackageId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(3)
            .ToListAsync();

        var trendingPackages = await _context.Packages
            .Where(p => trendingPackageIds.Contains(p.Id))
            .ToListAsync();

        var bookingsByMonth = await _context.Bookings
            .GroupBy(b => b.BookingDate.Month)
            .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());

        var viewModel = new DashboardViewModel
        {
            UserName = userName,
            TotalUsers = totalUsers,
            TotalBookings = totalBookings,
            TotalPackages = totalPackages,
            TrendingPackages = trendingPackages,
            BookingsByMonth = bookingsByMonth
        };

        return View(viewModel);
    }

    // Action to generate and download the report
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DownloadReport()
    {
        // 1. Collect all the data needed for the report
        var totalUsers = await _context.Users.CountAsync();
        var totalBookings = await _context.Bookings.CountAsync();
        var totalPackages = await _context.Packages.CountAsync();

        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Package)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        var packages = await _context.Packages
            .OrderBy(p => p.Title)
            .ToListAsync();

        // New queries for cancellations and refunds
        var cancelledBookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Package)
            .Where(b => b.Status == "Cancelled")
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        var totalCancellations = cancelledBookings.Count;
        var totalRefunds = cancelledBookings.Count(); // Assuming every cancellation gets a refund

        // 2. Generate the report as a PDF
        using (MemoryStream ms = new MemoryStream())
        {
            Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
            PdfWriter writer = PdfWriter.GetInstance(doc, ms);
            doc.Open();

            // Report Header
            doc.Add(new Paragraph("Tourism Management Platform - Admin Report"));
            doc.Add(new Paragraph($"Report Date: {DateTime.Now.ToString("dd-MM-yyyy")}"));
            doc.Add(new Paragraph("-------------------------------------------------"));
            doc.Add(new Paragraph("")); // Add a blank line

            // Summary Metrics
            doc.Add(new Paragraph("Platform Summary"));
            doc.Add(new Paragraph($"Total Registered Users: {totalUsers}"));
            doc.Add(new Paragraph($"Total Bookings: {totalBookings}"));
            doc.Add(new Paragraph($"Total Packages: {totalPackages}"));
            doc.Add(new Paragraph(""));
            doc.Add(new Paragraph("Cancellations & Refunds Summary"));
            doc.Add(new Paragraph($"Total Cancellations: {totalCancellations}"));
            doc.Add(new Paragraph($"Total Refunds: {totalRefunds}"));
            doc.Add(new Paragraph("-------------------------------------------------"));

            // Bookings Details
            doc.Add(new Paragraph("Recent Bookings Overview"));
            PdfPTable bookingsTable = new PdfPTable(4);
            bookingsTable.AddCell("Booking ID");
            bookingsTable.AddCell("User Email");
            bookingsTable.AddCell("Package Title");
            bookingsTable.AddCell("Booking Date");

            foreach (var booking in bookings.Take(15))
            {
                bookingsTable.AddCell(booking.Id.ToString());
                bookingsTable.AddCell(booking.User?.Email ?? "N/A");
                bookingsTable.AddCell(booking.Package?.Title ?? "N/A");
                bookingsTable.AddCell(booking.BookingDate.ToShortDateString());
            }
            doc.Add(bookingsTable);
            doc.Add(new Paragraph(""));
            doc.Add(new Paragraph("-------------------------------------------------"));

            // Packages Details
            doc.Add(new Paragraph("Package Inventory Overview"));
            PdfPTable packagesTable = new PdfPTable(4);
            packagesTable.AddCell("Package Title");
            packagesTable.AddCell("Destination");
            packagesTable.AddCell("Price");
            packagesTable.AddCell("Status");

            foreach (var packageItem in packages)
            {
                packagesTable.AddCell(packageItem.Title);
                packagesTable.AddCell(packageItem.Destination);
                packagesTable.AddCell($"₹{packageItem.Price}");
                packagesTable.AddCell(packageItem.Status);
            }
            doc.Add(packagesTable);
            doc.Add(new Paragraph(""));
            doc.Add(new Paragraph("-------------------------------------------------"));

            // Cancellations Details (New Section)
            doc.Add(new Paragraph("Cancelled Bookings Overview"));
            PdfPTable cancellationsTable = new PdfPTable(4);
            cancellationsTable.AddCell("Booking ID");
            cancellationsTable.AddCell("User Email");
            cancellationsTable.AddCell("Package Title");
            cancellationsTable.AddCell("Cancellation Date");

            foreach (var cancelledBooking in cancelledBookings)
            {
                cancellationsTable.AddCell(cancelledBooking.Id.ToString());
                cancellationsTable.AddCell(cancelledBooking.User?.Email ?? "N/A");
                cancellationsTable.AddCell(cancelledBooking.Package?.Title ?? "N/A");
                cancellationsTable.AddCell(cancelledBooking.BookingDate.ToShortDateString());
            }
            doc.Add(cancellationsTable);

            doc.Close();

            // 3. Return the file
            byte[] bytes = ms.ToArray();
            return File(bytes, "application/pdf", $"AdminReport_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.pdf");
        }
    }
}