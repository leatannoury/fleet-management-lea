//represents the db session
//tells ef core which tables exist
//map domain entities to db tables

using FleetService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetService.Infrastructure.Persistence;

public class FleetDbContext : DbContext
{
    //constructor
    public FleetDbContext(DbContextOptions<FleetDbContext> options)
        : base(options)
    {
    }

    //dbsets - database tables
    public DbSet<Vehicle> Vehicles { get; set; }

    public DbSet<Driver> Drivers { get; set; }
    
    //configure db schema
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //vehicle configuration
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(v => v.Id);//set ID as primary key

            entity.OwnsOne(v => v.PlateNumber, p =>
            {
                p.Property(x => x.Value)
                    .HasColumnName("PlateNumber")
                    .IsRequired();
            }); //plate number is a value object not a string. ef can't store objects directly,
                //so ownsone() tells ef to the the properties of the value object in the same table

            entity.HasIndex("PlateNumber").IsUnique();//make a unique index  so 2 plates can't have the same plate number
            
            //required properties - cant be null
            entity.Property(v => v.Model).IsRequired();
            entity.Property(v => v.Year).IsRequired();
            entity.Property(v => v.Type).IsRequired();
            entity.Property(v => v.Status).IsRequired();
            entity.Property(v => v.Mileage);
        });

        //driver configuration
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(d => d.Id); //id is primary key

            entity.OwnsOne(d => d.LicenseNumber, l =>
            {
                l.Property(x => x.Value)
                    .HasColumnName("LicenseNumber")
                    .IsRequired();
            });//license number is a value object not a string. ef can't store objects directly,
            //so ownsone() tells ef to the the properties of the value object in the same table

            entity.HasIndex("LicenseNumber").IsUnique();//make a unique index  so 2 drivers can't have the same license number
            
            //required fields- cant be null
            entity.Property(d => d.FirstName).IsRequired();
            entity.Property(d => d.LastName).IsRequired();
            entity.Property(d => d.Status).IsRequired();
        });
    }
}