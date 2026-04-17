using Airline_Ticket_System.Entities;
using Airline_Ticket_System.Repositories.Interfaces;
using Airline_Ticket_System.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace Airline_Ticket_System.Repositories;

public class FlightRepository : IFlightRepository
{
    private readonly ApplicationDbContext _db;

    public FlightRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Flight>> GetAllWithPassengersAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Flights
            .Include(f => f.FlightPassengers)
            .ThenInclude(fp => fp.Passenger)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Flight> Items, int TotalCount)> SearchAsync(FlightSearchCriteria c, CancellationToken cancellationToken = default)
    {
        var q = _db.Flights
            .AsNoTracking()
            .Include(f => f.FlightPassengers)
            .ThenInclude(fp => fp.Passenger)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(c.DepartureCity))
        {
            var d = c.DepartureCity.Trim();
            q = q.Where(f => f.DepartureCity.Contains(d));
        }

        if (!string.IsNullOrWhiteSpace(c.ArrivalCity))
        {
            var a = c.ArrivalCity.Trim();
            q = q.Where(f => f.ArrivalCity.Contains(a));
        }

        if (c.FromDate.HasValue)
        {
            var from = DateTime.SpecifyKind(c.FromDate.Value.Date, DateTimeKind.Utc);
            q = q.Where(f => f.DepartureDateTime >= from);
        }

        if (c.ToDate.HasValue)
        {
            var to = DateTime.SpecifyKind(c.ToDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            q = q.Where(f => f.DepartureDateTime < to);
        }

        if (c.MinPrice.HasValue)
            q = q.Where(f => f.Price >= c.MinPrice.Value);

        if (c.MaxPrice.HasValue)
            q = q.Where(f => f.Price <= c.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(c.Status))
        {
            var s = c.Status.Trim();
            q = q.Where(f => f.Status == s);
        }

        q = (c.SortBy?.ToLowerInvariant()) switch
        {
            "price_asc" => q.OrderBy(f => f.Price).ThenBy(f => f.DepartureDateTime),
            "price_desc" => q.OrderByDescending(f => f.Price).ThenBy(f => f.DepartureDateTime),
            "duration" => q.OrderBy(f => f.Duration).ThenBy(f => f.DepartureDateTime),
            "availability" => q.OrderByDescending(f => f.Capacity).ThenBy(f => f.DepartureDateTime),
            "flightnumber" => q.OrderBy(f => f.FlightNumber).ThenBy(f => f.DepartureDateTime),
            _ => q.OrderBy(f => f.DepartureDateTime).ThenBy(f => f.FlightNumber),
        };

        var total = await q.CountAsync(cancellationToken);
        var page = Math.Max(1, c.Page);
        var pageSize = Math.Clamp(c.PageSize, 1, 100);
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<Flight?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _db.Flights.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task AddAsync(Flight flight, CancellationToken cancellationToken = default)
    {
        await _db.Flights.AddAsync(flight, cancellationToken);
    }

    public void Remove(Flight flight)
    {
        _db.Flights.Remove(flight);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddFlightPassengerAsync(FlightPassenger booking, CancellationToken cancellationToken = default)
    {
        await _db.FlightPassengers.AddAsync(booking, cancellationToken);
    }
}
