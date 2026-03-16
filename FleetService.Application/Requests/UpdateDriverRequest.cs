//this represents the data the client send to update a driver

namespace FleetService.Application.Requests;

public class UpdateDriverRequest
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime LicenseExpiry { get; set; }
}