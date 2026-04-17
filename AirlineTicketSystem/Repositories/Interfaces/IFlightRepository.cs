using Airline_Ticket_System.Entities;
using Airline_Ticket_System.Repositories.Models;

namespace Airline_Ticket_System.Repositories.Interfaces;

/// <summary>
/// Flight aggregate persistence (query/command split at the DbContext boundary).
/// </summary>
public interface IFlightRepository
{
    Task<IReadOnlyList<Flight>> GetAllWithPassengersAsync(CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Flight> Items, int TotalCount)> SearchAsync(FlightSearchCriteria criteria, CancellationToken cancellationToken = default);

    Task<Flight?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Flight flight, CancellationToken cancellationToken = default);

    void Remove(Flight flight);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task AddFlightPassengerAsync(FlightPassenger booking, CancellationToken cancellationToken = default);
}
