using Xunit;
using Airline_Ticket_System.Entities;

namespace Airline_Ticket_System.Tests
{
    public class PassengerTests
    {
        [Fact]
        public void Passenger_Constructor_ShouldSetFirstNameAndFamilyName()
        {
            // Arrange
            var firstName = "Georgi";
            var familyName = "Petrov";

            // Act
            var passenger = new Passenger(firstName, familyName);

            // Assert
            Assert.Equal(firstName, passenger.FirstName);
            Assert.Equal(familyName, passenger.FamilyName);
        }

        [Fact]
        public void Passenger_FlightPassengers_ShouldBeNullByDefault()
        {
            // Arrange
            var passenger = new Passenger("Maria", "Ivanova");

            // Act & Assert
            Assert.Null(passenger.FlightPassengers);
        }
    }
}

