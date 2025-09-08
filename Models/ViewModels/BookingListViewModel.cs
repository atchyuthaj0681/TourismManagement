using System.Collections.Generic;
using TourismManagement.Models;

namespace TourismManagement.Models.ViewModels
{
    public class BookingListViewModel
    {
        public List<Booking> Bookings { get; set; } = new();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public string? UserEmail { get; set; }
        public string? Status { get; set; }
        public int? PackageId { get; set; }
        public string SortOrder { get; set; } = "";

        public List<Package>? Packages { get; set; } = new();
    }
}
