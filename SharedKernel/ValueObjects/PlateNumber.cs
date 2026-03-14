//plate numbers

using SharedKernel.Common;

namespace SharedKernel.ValueObjects;

public class PlateNumber : ValueObject
{
    //properties
    public string Value { get; }

    //enforce rules
    //plate number can't be empty
    //ex: PlateNumber("") throws an exception
    public PlateNumber(string value) //constructor
    {
        if (string.IsNullOrWhiteSpace(value)) //check if its value is null or empty
            throw new ArgumentException("Plate number cannot be empty"); //throw the exception

        Value = value;
    }

    //implement GetEqualityComponents method from valueobject class
    //check if 2 plate numbers are equal 
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}