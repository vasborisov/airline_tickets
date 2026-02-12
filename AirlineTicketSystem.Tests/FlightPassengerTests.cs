using System;
using Xunit;
using Airline_Ticket_System.Entities;
using Airline_Ticket_System.Data.Entities;

namespace Airline_Ticket_System.Tests
{
    public class FlightPassengerTests
    {
        [Fact]
        public void FlightPassenger_DefaultValues_ShouldSetCreatedAt()
        {
            // Act
            var flightPassenger = new FlightPassenger();

            // Assert
            Assert.True((DateTime.UtcNow - flightPassenger.CreatedAt).TotalSeconds < 5);
        }

        [Fact]
        public void FlightPassenger_SetProperties_ShouldStoreValuesCorrectly()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user123", FirstName = "Ivan", FamilyName = "Ivanov" };
            var passenger = new Passenger("Test", "Passenger"); 
            var flight = new Flight(10, "Sofia", "Paris", 120, 250, 180);

            // Act
            var flightPassenger = new FlightPassenger
            {
                Id = 1,
                FlightId = flight.Id,
                Flight = flight,
                PassengerId = passenger.Id,
                Passenger = passenger,
                CreatedByUserId = user.Id,
                CreatedByUser = user
            };

            // Assert
            Assert.Equal(1, flightPassenger.Id);
            Assert.Equal(10, flightPassenger.FlightId);
            Assert.Equal("Sofia", flightPassenger.Flight?.DepartureCity);  
            Assert.Equal("Test", flightPassenger.Passenger?.FirstName);
            Assert.Equal("Passenger", flightPassenger.Passenger?.FamilyName);
            Assert.Equal("user123", flightPassenger.CreatedByUserId);
            Assert.Equal("Ivan", flightPassenger.CreatedByUser?.FirstName);
        }
    }
}
