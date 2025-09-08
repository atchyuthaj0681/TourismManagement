using System.Collections.Generic;

namespace TourismManagement.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalBookings { get; set; }
        public int TotalDestinations { get; set; }
        public int TotalPeople { get; set; }
        public Dictionary<string, int> BookingCountByDestination { get; set; }
    }

}
