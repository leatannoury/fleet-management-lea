//reporsitory seperate application layer from ef core
//services dont call dbcontext directly
//instead: VehicleService → VehicleRepository → DbContext

//database access

using FleetService.Domain.Entities;
using FleetService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FleetService.Application.Interfaces;

namespace FleetService.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
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