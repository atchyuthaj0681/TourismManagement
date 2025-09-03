using System.ComponentModel.DataAnnotations;

namespace TourismManagement.Models
{
    public class Destination
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Country { get; set; }

        [Required]
        public string Description { get; set; }

        public string ImageUrl { get; set; } // for displaying pictures

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public string BestTimeToVisit { get; set; }

        public int DurationDays { get; set; }
        public string Category { get; set; }

        public ICollection<DestinationImage> Images { get; set; } = new List<DestinationImage>();

    }

}
