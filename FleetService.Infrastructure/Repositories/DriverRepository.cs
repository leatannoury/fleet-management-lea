//reporsitory seperate application layer from ef core
//services dont call dbcontext directly
//instead: driver Service → driver Repository → DbContext

//database access

using FleetService.Domain.Entities;
using FleetService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FleetService.Application.Interfaces;

namespace FleetService.Infrastructure.Repositories;

public class DriverRepository: IDriverRepository
{
    private readonly FleetDbContext _context;//db connection object

    //constructor - dependency injection
    public DriverRepository(FleetDbContext context)
    {
        _context = context;
    }

    //add driver to db
    public async Task AddAsync(Driver driver)
    {
        await _context.Drivers.AddAsync(driver);
        await _context.SaveChangesAsync();
    }

    //retreive all drivers from db
    public async Task<List<Driver>> GetAllAsync()
    {
        return await _context.Drivers.ToListAsync();
    }
}