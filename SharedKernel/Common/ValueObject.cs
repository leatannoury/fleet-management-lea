//value objects are objects without identity => no id needed 
//they are defined by their attributes instead of a unique id
// value objects are equal if their content is equal
//location, money, plate number, license number
//we do value objects to enforce rules so invalid data cant enter the domain

namespace SharedKernel.Common;

public abstract class ValueObject
{
    //each value object must specify wich properties make it unique
    //ex: location depends on latitude, lontitude, address
    protected abstract IEnumerable<object> GetEqualityComponents(); //class inheriting from valueobject should implement it
    //they should specify a list of their properties to compare

    public override bool Equals(object obj) //compare by value (not by memory addr)
    {
        if (obj == null || obj.GetType() != GetType()) //see if the obj is empty or of another type
            return false;

        var other = (ValueObject)obj;//cast the obj to a value object so we access its components

        return GetEqualityComponents() 
            .SequenceEqual(other.GetEqualityComponents()); //compare the lists of properties to see if they match if yes -> return true
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) =>
            {
                return current * 23 + (obj?.GetHashCode() ?? 0);
            });
    }
}