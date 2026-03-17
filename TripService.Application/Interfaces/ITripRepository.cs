//interface for trip
//so we don't depend on infrastructure directly
//we define methods we want to do in trip

using TripService.Domain.Entities;

namespace TripService.Application.Interfaces;

public interface ITripRepository
{
    Task AddAsync(Trip trip); //add a trip
    Task<Trip?> GetByIdAsync(Guid id); //get a trip by id
    Task UpdateAsync(Trip trip); //update a trip
}