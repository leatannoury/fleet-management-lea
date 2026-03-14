//location : latitude, longtitude, address
//represents a geographic location
//for trip tracking

using SharedKernel.Common;

namespace SharedKernel.ValueObjects;

public class Location : ValueObject
{
    //prop of a location
    
    public double Latitude { get; }
    public double Longitude { get; }
    public string Address { get; }
    

    //constructor
    public Location(double latitude, double longitude, string address)
    {
        Latitude = latitude;
        Longitude = longitude;
        Address = address;
    }

    //implement GetEqualityComponents method from valueobject class
    //check if 2 locations are equal if they have same latitude, longtitude, address.
    protected override IEnumerable<object> GetEqualityComponents() 
    {
        yield return Latitude;
        yield return Longitude;
        yield return Address;
    }
}