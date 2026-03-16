//interface so application doesnt depend on infrastructure directly
//define operations  on drivers

using FleetService.Domain.Entities;

namespace FleetService.Application.Interfaces;

public interface IDriverRepository
{
    Task AddAsync(Driver driver);//add driver
    Task<List<Driver>> GetAllAsync();//get all drivers
    Task<Driver?> GetByIdAsync(Guid id);//get driver by id
    Task UpdateAsync(Driver driver);//update a driver
    Task DeleteAsync(Driver driver);//delete a driver
}