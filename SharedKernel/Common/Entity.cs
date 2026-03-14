//entity=object with identity
//entities are different even if they have same data (they have diff id)
//vehicle, driver, trip, triplog
//ex: cars can have same model, year, color, but they are still different cars each one has an ID => different entities

namespace SharedKernel.Common;


public abstract class Entity //other classes inherit from it
{
    public Guid Id { get; protected set; } //each entity have an identifier/ guid: globally unique

    protected Entity() //when a new entity is created a id is generated
    {
        Id = Guid.NewGuid();
    }
}