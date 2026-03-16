//represents vehicle lifecycle state 
//lifecycle: Registered → Active → InMaintenance → Decommissioned

//Registered:created in the system but not yet operational
//active: vehicle available for operations
//in maintenance: being repaired
//decommissioned: retired permanently

namespace FleetService.Domain.Enums;

public enum VehicleStatus
{
    Registered,
    Active,
    InMaintenance,
    Decommissioned
}