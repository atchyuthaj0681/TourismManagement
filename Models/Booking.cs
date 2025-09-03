using System.ComponentModel.DataAnnotations;

namespace TourismManagement.Models
{

    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int PackageId { get; set; }
        public Package Package { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }   // Navigation property

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public DateTime TourDate { get; set; }

        [Required]
        public int NumberOfPeople { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        public string PaymentStatus { get; set; } // e.g., Paid, Pending, Refunded

        public string BookingStatus { get; set; } // e.g., Confirmed, Cancelled, Completed
    }
}
