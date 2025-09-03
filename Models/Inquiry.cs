using System;
using System.ComponentModel.DataAnnotations;

namespace TourismManagement.Models
{
    public class Inquiry
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsResolved { get; set; }
    }
}
