//reporsitory seperate application layer from ef core
//bridge between application and db
//services dont call dbcontext directly
//instead: trip Service → trip Repository → DbContext

//db access

using Microsoft.EntityFrameworkCore;
using TripService.Application.Interfaces;
using TripService.Domain.Entities;
using TripService.Infrastructure.Persistence;

public class TripLogRepository : ITripLogRepository
{
    private readonly TripDbContext _context;//db connection object

    public TripLogRepository(TripDbContext context)
    {
        _context = context;
    }

    //add trip log to db
    public async Task AddAsync(TripLog log)
    {
        await _context.TripLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    //get logs for a trip
    public async Task<List<TripLog>> GetByTripIdAsync(Guid tripId)
    {
        return await _context.TripLogs
            .Where(l => l.TripId == tripId)
            .ToListAsync();
    }
}