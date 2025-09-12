using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // 🧠 Support for multiple images
        [NotMapped]
        public List<string> ImagePaths { get; set; } = new List<string>();

        // 👇 This is stored in DB
        public string? ImagePathString
        {
            get => string.Join(",", ImagePaths);
            set => ImagePaths = value?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
        }

        // ❗ Optional: Keep this if used in old views/controllers
        [Display(Name = "Package Image")]
        public string? ImagePath { get; set; }

        // Navigation property
        public List<Booking> Bookings { get; set; } = new List<Booking>();

        public List<PackageImage> Images { get; set; } = new List<PackageImage>();

    }
}
