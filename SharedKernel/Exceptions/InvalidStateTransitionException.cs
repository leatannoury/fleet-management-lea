//used for the vehicle state
//vehicle state: registered, active, in maintenance, decommissioned -> we can't jump randomly between them
//

namespace SharedKernel.Exceptions;

public class InvalidStateTransitionException : DomainException
{
    public InvalidStateTransitionException(string message)
        : base(message) //allow custom messagss
    {
    }
}