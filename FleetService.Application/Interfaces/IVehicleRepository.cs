//interface so application doesnt depend on infrastructure directly
//define operations  on vehicles
 
using FleetService.Domain.Entities;

namespace FleetService.Application.Interfaces;

public interface IVehicleRepository
{
    Task AddAsync(Vehicle vehicle); //add a vehicle
    Task<List<Vehicle>> GetAllAsync();//get all vehicle
    Task<Vehicle?> GetByIdAsync(Guid id);//get vehicle by id
}