//request (command input ) for adding location log \
//it represents user input


namespace TripService.Application.Requests;

public class AddLocationLogRequest
{
    public Guid TripId { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; }

    public decimal? Speed { get; set; }
}