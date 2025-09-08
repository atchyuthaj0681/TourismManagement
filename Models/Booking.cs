using TourismManagement.Models;
namespace TourismManagement.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int PackageId { get; set; }
        public Package Package { get; set; }

        public DateTime BookingDate { get; set; }
        public DateTime TravelDate { get; set; }
        public int NumPeople { get; set; }
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Booked"; // or Cancelled
        public DateTime? CancellationDate { get; set; }
        public decimal? RefundAmount { get; set; }
    }
}