//reporsitory seperate application layer from ef core
//bridge between application and db
//services dont call dbcontext directly
//instead: trip Service → trip Repository → DbContext

//db access

using Microsoft.EntityFrameworkCore;
using TripService.Application.Interfaces;
using TripService.Domain.Entities;
using TripService.Infrastructure.Persistence;

namespace TripService.Infrastructure.Repositories;

public class TripRepository : ITripRepository
{
    private readonly TripDbContext _context;//db connection object

    public TripRepository(TripDbContext context)
    {
        _context = context;
    }

    //add trip to db
    public async Task AddAsync(Trip trip)
    {
        await _context.Trips.AddAsync(trip);
        await _context.SaveChangesAsync();
    }

    //get trip by id
    public async Task<Trip?> GetByIdAsync(Guid id)
    {
        return await _context.Trips.FirstOrDefaultAsync(t => t.Id == id);
    }

    //update trip
    public async Task UpdateAsync(Trip trip)
    {
        _context.Trips.Update(trip);
        await _context.SaveChangesAsync();
    }
}