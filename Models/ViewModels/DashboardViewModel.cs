// File: TourismManagement.Models.ViewModels.DashboardViewModel.cs

using System.Collections.Generic;
using TourismManagement.Models;

namespace TourismManagement.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string UserName { get; set; }
        public int TotalUsers { get; set; }
        public int TotalBookings { get; set; }
        public int TotalPackages { get; set; } // Add this line
        public List<Package> TrendingPackages { get; set; }
        public Dictionary<string, int> BookingsByMonth { get; set; }
    }
}