//represents the db session of trip
//tells ef core which tables exist
//map domain entities to db tables

using Microsoft.EntityFrameworkCore;
using TripService.Domain.Entities;

namespace TripService.Infrastructure.Persistence;

public class TripDbContext : DbContext
{
    //constructor-dependency injection
    public TripDbContext(DbContextOptions<TripDbContext> options)
        : base(options)
    {
    }

    //tables in the db
    public DbSet<Trip> Trips { get; set; }
    public DbSet<TripLog> TripLogs { get; set; }

    //configure db schema
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //trip configuration
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(t => t.Id);

            // value objects 
            entity.OwnsOne(t => t.StartLocation, loc =>
            {
                loc.Property(l => l.Latitude).HasColumnName("StartLat");
                loc.Property(l => l.Longitude).HasColumnName("StartLng");
                loc.Property(l => l.Address).HasColumnName("StartAddress");
            });

            entity.OwnsOne(t => t.EndLocation, loc =>
            {
                loc.Property(l => l.Latitude).HasColumnName("EndLat");
                loc.Property(l => l.Longitude).HasColumnName("EndLng");
                loc.Property(l => l.Address).HasColumnName("EndAddress");
            }); //location and money is a value object not a string. ef can't store objects directly,
            //so ownsone() tells ef to the the properties of the value object in the same table

            entity.OwnsOne(t => t.Cost, cost =>
            {
                cost.Property(c => c.Amount).HasColumnName("CostAmount");
                cost.Property(c => c.Currency).HasColumnName("CostCurrency");
            });

            // enum status
            entity.Property(t => t.Status);
        });

        //trip log configuration
        modelBuilder.Entity<TripLog>(entity =>
        {
            entity.HasKey(l => l.Id); //pk
            
            entity.HasOne<Trip>()
                .WithMany()
                .HasForeignKey(l => l.TripId)
                .OnDelete(DeleteBehavior.Cascade); //trip foreign key 

            entity.OwnsOne(l => l.Location, loc =>
            {
                loc.Property(l => l.Latitude).HasColumnName("Lat");
                loc.Property(l => l.Longitude).HasColumnName("Lng");
                loc.Property(l => l.Address).HasColumnName("Address");
            });//value object
        });
    }
}