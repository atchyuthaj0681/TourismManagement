using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TourismManagement.Models
{
    public class Package
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Package title is required.")]
        [StringLength(100)]
        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Destination is required.")]
        [StringLength(100)]
        public string Destination { get; set; }

        [Range(1, 365, ErrorMessage = "Duration must be between 1 and 365 days.")]
        public int DurationDays { get; set; }

        [Range(0, 100000, ErrorMessage = "Price must be a positive value.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [DataType(DataType.MultilineText)]
        public string Itinerary { get; set; }

        [Required]
        [StringLength(10)]
        public string Status { get; set; } // "Active" or "Inactive"

        // Navigation property
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
