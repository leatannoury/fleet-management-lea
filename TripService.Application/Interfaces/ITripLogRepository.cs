//interface for trip log
//so we don't depend on infrastructure directly
//we define methods we want to do in triplog

namespace TripService.Application.Interfaces;

using TripService.Domain.Entities;

public interface ITripLogRepository
{
    Task AddAsync(TripLog log); //add a trip log
    Task<List<TripLog>> GetByTripIdAsync(Guid tripId); //get logs for a trip
}