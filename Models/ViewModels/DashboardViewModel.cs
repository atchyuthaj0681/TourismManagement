using System.Collections.Generic;

namespace TourismManagement.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalBookings { get; set; }
        public int TotalDestinations { get; set; }

        public int TotalPeople { get; set; } // ✅ This fixes CS1061
        public Dictionary<string, int> BookingCountByDestination { get; set; } // ✅ This too
    }
}
