//entities are grouped in aggregates - system interacts with the root of the aggregate
//ex: trip = aggregate root - triplog = internal object
//instead of updatinh triplog directly we update from trip 
//vehicle, driver, trip

namespace SharedKernel.Common;

public abstract class AggregateRoot : Entity  //aggregate root is an entity -> directly inherits ID
{
}