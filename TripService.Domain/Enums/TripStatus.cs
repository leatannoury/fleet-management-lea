//statuses of the trip
//represent the lifecycle of the trip
//lifecycle: Requested → Assigned → InProgress → Completed → Invoiced

namespace TripService.Domain.Enums;

public enum TripStatus
{
    Requested = 0,
    Assigned = 1,
    InProgress = 2,
    Completed = 3,
    Invoiced = 4
}