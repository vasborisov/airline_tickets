using Airline_Ticket_System.Entities;
using Airline_Ticket_System.Repositories.Models;
using System.Collections.Generic;

namespace Airline_Ticket_System.Services.Interfaces
{
    public interface IFlightService
    {
        Task AddFlightAsync(Flight flight);

        Task DeleteFlightAsync(Flight flight);

        Task<IEnumerable<Flight>> LoadAllFlightsAsync();

        Task<(IReadOnlyList<Flight> Items, int TotalCount)> SearchFlightsAsync(FlightSearchCriteria criteria, CancellationToken cancellationToken = default);

        Task BookSeatAsync(Flight flight, Passenger passenger);

        Task CancelBookedSeatAsync();
    }
}
