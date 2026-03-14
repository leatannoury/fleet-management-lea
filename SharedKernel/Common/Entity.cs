//entity=object with identity
//entities are different even if they have same data (they have diff id)
//vehicle, driver, trip, triplog


namespace SharedKernel.Common;


public abstract class Entity //other classes inherit from it
{
    public Guid Id { get; protected set; } //each entity have an identifier/ guid: globally unique

    protected Entity() //when a new entity is created a id is generated
    {
        Id = Guid.NewGuid();
    }
}