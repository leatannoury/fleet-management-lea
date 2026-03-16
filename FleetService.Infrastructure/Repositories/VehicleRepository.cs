//reporsitory seperate application layer from ef core
//services dont call dbcontext directly
//instead: VehicleService → VehicleRepository → DbContext

//database access

//get the request from service and interact with the db

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

    //add vehicle to db
    public async Task AddAsync(Vehicle vehicle)
    {
        await _context.Vehicles.AddAsync(vehicle);//add the vehicle in the vehicle table in db
        await _context.SaveChangesAsync();//save
    }

    //retreive all vehicles from db
    public async Task<List<Vehicle>> GetAllAsync()
    {
        return await _context.Vehicles.ToListAsync();
    }
    
    //get vehicle by id
    public async Task<Vehicle?> GetByIdAsync(Guid id)
    {
        return await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
    }
    
    //update a vehicle
    public async Task UpdateAsync(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);//update
        await _context.SaveChangesAsync();//save
    }

    //delete a vehicle
    public async Task DeleteAsync(Vehicle vehicle)
    {
        _context.Vehicles.Remove(vehicle);//delete
        await _context.SaveChangesAsync();//save
    }
}