//reporsitory seperate application layer from ef core
//services dont call dbcontext directly
//instead: driver Service → driver Repository → DbContext

using FleetService.Domain.Entities;
using FleetService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FleetService.Infrastructure.Repositories;

public class DriverRepository
{
    private readonly FleetDbContext _context;//db connection object

    //constructor - dependency injection
    public DriverRepository(FleetDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Driver driver)//add driver to db
    {
        await _context.Drivers.AddAsync(driver);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Driver>> GetAllAsync()//retreive all drivers from db
    {
        return await _context.Drivers.ToListAsync();
    }
}