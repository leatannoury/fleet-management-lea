//represents vehicle lifecycle state 
//lifecycle: Registered → Active → InMaintenance → Decommissioned

namespace FleetService.Domain.Enums;

public enum VehicleStatus
{
    Registered,
    Active,
    InMaintenance,
    Decommissioned
}