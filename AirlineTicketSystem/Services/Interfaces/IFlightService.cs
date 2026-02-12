using Airline_Ticket_System.Entities;
using System.Collections.Generic;

namespace Airline_Ticket_System.Services.Interfaces
{
    public interface IFlightService
    {
        Task AddFlight(Flight flight);

        Task DeleteFlightAsync(Flight flight);

        Task<IEnumerable<Flight>> LoadAllFlightsAsync();
        Task BookSeatAsync(Flight flight, Passenger passenger);
        void CancelBookedSeat();
    }
}
