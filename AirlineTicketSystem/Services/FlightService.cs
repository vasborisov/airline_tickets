using Airline_Ticket_System.Entities;
using Airline_Ticket_System.Repositories.Interfaces;
using Airline_Ticket_System.Repositories.Models;
using Airline_Ticket_System.Services.Interfaces;

namespace Airline_Ticket_System.Services
{
    public class FlightService : IFlightService
    {
        private readonly IFlightRepository _flights;

        public FlightService(IFlightRepository flights)
        {
            _flights = flights;
        }

        public async Task AddFlightAsync(Flight newFlightEntity)
        {
            await _flights.AddAsync(newFlightEntity);
            await _flights.SaveChangesAsync();
        }

        public async Task DeleteFlightAsync(Flight flight)
        {
            if (flight != null)
            {
                _flights.Remove(flight);
                await _flights.SaveChangesAsync();
            }
        }

        public async Task BookSeatAsync(Flight flight, Passenger passenger)
        {
            flight.Capacity -= 1;

            var flightPassenger = new FlightPassenger
            {
                FlightId = flight.Id,
                PassengerId = passenger.Id
            };

            await _flights.AddFlightPassengerAsync(flightPassenger);
            await _flights.SaveChangesAsync();
        }

        public async Task<IEnumerable<Flight>> LoadAllFlightsAsync()
        {
            var list = await _flights.GetAllWithPassengersAsync();
            return list;
        }

        public Task<(IReadOnlyList<Flight> Items, int TotalCount)> SearchFlightsAsync(FlightSearchCriteria criteria, CancellationToken cancellationToken = default)
            => _flights.SearchAsync(criteria, cancellationToken);

        public Task CancelBookedSeatAsync() => Task.CompletedTask;
    }
}
