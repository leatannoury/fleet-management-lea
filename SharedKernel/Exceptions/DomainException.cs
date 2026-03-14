//base class
//domain exception=error happens when when a business rule is broken 

namespace SharedKernel.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}