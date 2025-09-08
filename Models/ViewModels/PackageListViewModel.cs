 using System.Collections.Generic;
   //using TourismManagement.Models;
namespace TourismManagement.Models.ViewModels
{


    public class PackageListViewModel
    {
        public IEnumerable<Package> Packages { get; set; } // All packages (for scroll)
        public IEnumerable<Package> TrendingPackages { get; set; } // For top trending grid
        public string SearchString { get; set; }
        public string StatusFilter { get; set; }
        public PaginationModel Pagination { get; set; }
    }

}
