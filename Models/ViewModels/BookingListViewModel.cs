namespace TourismManagement.Models.ViewModels
{
    public class BookingListViewModel
    {
        public IEnumerable<Booking> Bookings { get; set; }
        public string SearchString { get; set; }
        public string PaymentFilter { get; set; }
        public string BookingFilter { get; set; }
        public PaginationModel Pagination { get; set; }
    }
}
