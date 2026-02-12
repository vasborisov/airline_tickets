using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirlineTicketSystem.Tests
{
    using Airline_Ticket_System.Models.Flight;
    using Airline_Ticket_System.Models.Passenger;
    using System.Collections.Generic;
    using Xunit;

    public class FlightViewModelTests
    {
        [Fact]
        public void Constructor_WithAllParametersIncludingPassengers_SetsPropertiesCorrectly()
        {
            // Arrange
            var passengers = new List<PassengerViewModel>
        {
            new PassengerViewModel(1, "John", "Doe"),
            new PassengerViewModel(2, "Jane", "Smith")
        };

            // Act
            var model = new FlightViewModel(1, "New York", "London", 360, 500.0m, 200, false, passengers);

            // Assert
            Assert.Equal(1, model.Id);
            Assert.Equal("New York", model.DepartureCity);
            Assert.Equal("London", model.ArrivalCity);
            Assert.Equal(360, model.Duration);
            Assert.Equal(500.0m, model.Price);
            Assert.Equal(200, model.Capacity);
            Assert.False(model.IsFullyBooked);
            Assert.Equal(passengers, model.PassengerViewModels);
        }

        [Fact]
        public void Constructor_WithoutPassengerList_SetsPropertiesCorrectly()
        {
            // Act
            var model = new FlightViewModel(2, "Paris", "Berlin", 120, 150.0m, 100, true);

            // Assert
            Assert.Equal(2, model.Id);
            Assert.Equal("Paris", model.DepartureCity);
            Assert.Equal("Berlin", model.ArrivalCity);
            Assert.Equal(120, model.Duration);
            Assert.Equal(150.0m, model.Price);
            Assert.Equal(100, model.Capacity);
            Assert.True(model.IsFullyBooked);
            Assert.Null(model.PassengerViewModels);
        }

        [Fact]
        public void Constructor_MinimalParameters_SetsPropertiesCorrectly()
        {
            // Act
            var model = new FlightViewModel(3, "Tokyo", "Seoul", 150, 300.0m, 80);

            // Assert
            Assert.Equal(3, model.Id);
            Assert.Equal("Tokyo", model.DepartureCity);
            Assert.Equal("Seoul", model.ArrivalCity);
            Assert.Equal(150, model.Duration);
            Assert.Equal(300.0m, model.Price);
            Assert.Equal(80, model.Capacity);
            Assert.Null(model.PassengerViewModels);
        }

        [Fact]
        public void SearchDepartureCity_Property_CanBeSetAndRead()
        {
            // Arrange
            var model = new FlightViewModel(4, "Rome", "Madrid", 90, 200.0m, 70);
            var expectedSearchCity = "Rome";

            // Act
            model.SearchDeparetureCity = expectedSearchCity;

            // Assert
            Assert.Equal(expectedSearchCity, model.SearchDeparetureCity);
        }
    }

}
