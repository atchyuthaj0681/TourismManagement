using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TourismManagement.Models;

namespace TourismManagement.Models.ViewModels
{
    public class BookingFormViewModel
    {
        [Required(ErrorMessage = "Please select a package")]
        public int PackageId { get; set; }

        [Required(ErrorMessage = "Travel date is required")]
        [DataType(DataType.Date)]
        public DateTime TravelDate { get; set; }

        [Required(ErrorMessage = "Number of people is required")]
        [Range(1, 100, ErrorMessage = "Number of people must be at least 1")]
        public int NumPeople { get; set; }

        public List<Package> AvailablePackages { get; set; } = new();
    }
}
