using Airline_Ticket_System.Entities;
using Airline_Ticket_System.Models.Flight;
using Airline_Ticket_System.Models.Passenger;
using Airline_Ticket_System.Repositories;
using Airline_Ticket_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Airline_Ticket_System.Services
{
    public class FlightService : IFlightService
    {
        private readonly ApplicationDbContext _context;

        public FlightService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddFlight(Flight newFlightEntity)
        {
            await _context.Flights.AddAsync(newFlightEntity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFlightAsync(Flight flight)
        {
            if (flight != null)
            {
                _context.Flights.Remove(flight);
                await _context.SaveChangesAsync();
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

            await _context.FlightPassengers.AddAsync(flightPassenger);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Flight>> LoadAllFlightsAsync()
        {
            return await _context.Flights
                        .Include(f => f.FlightPassengers)
                        .ThenInclude(fp => fp.Passenger)
                        .ToListAsync();
        }

        public void CancelBookedSeat()
        {
            throw new System.NotImplementedException();
        }
    }
}
