//reporsitory seperate application layer from ef core
//services dont call dbcontext directly
//instead: VehicleService → VehicleRepository → DbContext

using FleetService.Domain.Entities;
using FleetService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FleetService.Infrastructure.Repositories;

public class VehicleRepository
{
    private readonly FleetDbContext _context; //db connection object

    //constructor - dependency injection
    public VehicleRepository(FleetDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Vehicle vehicle)//add vehicle to db
    {
        await _context.Vehicles.AddAsync(vehicle);//add the vehicle in the vehicle table in db
        await _context.SaveChangesAsync();
    }

    public async Task<List<Vehicle>> GetAllAsync()//retreive all vehicles from db
    {
        return await _context.Vehicles.ToListAsync();
    }
}