//to log gps updates
//

using SharedKernel.Common;
using SharedKernel.ValueObjects;

namespace TripService.Domain.Entities;

public class TripLog : Entity
{
    //properties
    public Guid TripId { get; private set; }

    public Location Location { get; private set; }//value object

    public DateTime Timestamp { get; private set; }

    public decimal? Speed { get; private set; }

    //ef core constructor
    private TripLog() { }

    //constructor
    public TripLog(Guid tripId, Location location, decimal? speed)
    {
        TripId = tripId;
        Location = location;
        Speed = speed;
        Timestamp = DateTime.UtcNow;
    }
}