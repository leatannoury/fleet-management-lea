//license numbers

using SharedKernel.Common;

namespace SharedKernel.ValueObjects;

public class LicenseNumber : ValueObject
{
    //properties
    public string Value { get; }
    
    //constructor
    //rule: license number can't be empty
    public LicenseNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) //check if its empty
            throw new ArgumentException("License number cannot be empty"); //exception

        Value = value;
    }

    //implement GetEqualityComponents method from valueobject class
    //check if 2 license numbers are equal 
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}