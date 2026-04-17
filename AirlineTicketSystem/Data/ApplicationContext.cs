using Airline_Ticket_System.Data.Entities;
using Airline_Ticket_System.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Airline_Ticket_System.Repositories
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<FlightPassenger> FlightPassengers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Flight>()
           .Property(f => f.Price)
           .HasPrecision(18, 2);

            modelBuilder.Entity<FlightPassenger>()
                .HasIndex(fp => new { fp.FlightId, fp.PassengerId })
                .IsUnique()
                .HasFilter("[BookingStatus] = N'Confirmed'")
                .HasDatabaseName("IX_FlightPassengers_FlightId_PassengerId_Active_Unique");

            modelBuilder.Entity<FlightPassenger>()
                .HasIndex(fp => fp.Pnr)
                .IsUnique()
                .HasDatabaseName("IX_FlightPassengers_Pnr_Unique");

            modelBuilder.Entity<FlightPassenger>()
                .Property(fp => fp.PaymentAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FlightPassenger>()
                .Property(fp => fp.RefundAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Flight>()
                .HasIndex(f => f.DepartureCity)
                .HasDatabaseName("IX_Flights_DepartureCity");

            modelBuilder.Entity<Flight>()
                .HasIndex(f => f.ArrivalCity)
                .HasDatabaseName("IX_Flights_ArrivalCity");

            modelBuilder.Entity<Flight>()
                .HasIndex(f => f.DepartureDateTime)
                .HasDatabaseName("IX_Flights_DepartureDateTime");

            modelBuilder.Entity<FlightPassenger>()
                .HasIndex(fp => fp.CreatedAt)
                .HasDatabaseName("IX_FlightPassengers_CreatedAt");

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);
        }
    }
}