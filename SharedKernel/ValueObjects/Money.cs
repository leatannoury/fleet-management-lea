//cost of a trip 


using SharedKernel.Common;

namespace SharedKernel.ValueObjects;

public class Money : ValueObject
{
    //properties, ex: amount=25, currency=USD
    public decimal Amount { get; }
    public string Currency { get; }

    //constructor
    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    //implement GetEqualityComponents method from valueobject class
    //check if 2 money are equal ex: money(25 USD)=money(25 USD)
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}