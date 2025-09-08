using System.ComponentModel.DataAnnotations;

namespace TourismManagement.Models.ViewModels
{
    public class EditBookingViewModel
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Travel Date")]
        public DateTime TravelDate { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Number of people must be at least 1.")]
        [Display(Name = "Number of People")]
        public int NumPeople { get; set; }

        public decimal TotalAmount { get; set; }  // read-only display
    }


}
