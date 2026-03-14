//driver cant be assigned to a vehicle if his license expired

namespace SharedKernel.Exceptions;

public class DriverLicenseExpiredException : DomainException
{
    public DriverLicenseExpiredException()
        : base("Driver license is expired.")
    {
    }
}