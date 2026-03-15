//request model
//represent data coming FROM the client (post)

namespace FleetService.Application.Requests;

public class CreateDriverRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string LicenseNumber { get; set; }
    public DateTime LicenseExpiry { get; set; }
}