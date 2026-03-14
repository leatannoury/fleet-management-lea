//mapper
//convert model to dto

namespace SharedKernel.Mapping;

public static class MappingExtensions
{
    public static TDestination MapTo<TDestination>(this object source) //copies properties between objects
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source)); //check if null before continuing

        var destination = Activator.CreateInstance<TDestination>();//create empty instance of destination class

        var sourceProperties = source.GetType().GetProperties();//scan source object, get all its properties
        var destinationProperties = typeof(TDestination).GetProperties();//scan desti class, to see its prop

        foreach (var sourceProperty in sourceProperties) //loop in the properties of the src obj
        {
            var destProperty = destinationProperties
                .FirstOrDefault(p => p.Name == sourceProperty.Name &&
                                     p.PropertyType == sourceProperty.PropertyType);//find match in dest obj (name and datatype should be same)

            if (destProperty != null && destProperty.CanWrite)//if match found, and dest property is editable
            {
                var value = sourceProperty.GetValue(source);//take data from src obj property 
                destProperty.SetValue(destination, value);//put the data in the dest obj property
            }
        }

        return destination; //return the new obj
    }
}