// Models/PackageImage.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismManagement.Models
{
    public class PackageImage
    {
        public int Id { get; set; }

        [Required]
        public string ImagePath { get; set; }

        // Foreign key to Package
        public int PackageId { get; set; }

        [ForeignKey("PackageId")]
        public Package Package { get; set; }
    }
}
