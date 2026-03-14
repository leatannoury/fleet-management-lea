

namespace SharedKernel.Exceptions;

public class VehicleNotAvailableException : DomainException //VehicleNotAvailableException is a type of DomainException
{
    //constructor
    public VehicleNotAvailableException()
        : base("Vehicle is not available for this operation.") //sends msg to DomainException
    {
    }
}