using TourismManagement.Models;

public class DestinationImage
{
    public int Id { get; set; }
    public string ImageUrl { get; set; }

    public int DestinationId { get; set; }
    public Destination Destination { get; set; }
}

