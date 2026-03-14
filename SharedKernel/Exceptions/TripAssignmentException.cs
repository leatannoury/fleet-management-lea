//trip can only be assigned to active vehicle and available driver
//if not -> error

namespace SharedKernel.Exceptions;

public class TripAssignmentException : DomainException
{
    public TripAssignmentException(string message)
        : base(message)
    {
    }
}