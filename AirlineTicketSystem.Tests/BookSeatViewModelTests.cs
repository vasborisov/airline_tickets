using Xunit;
using Airline_Ticket_System.Models.Booking;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Airline_Ticket_System.Tests
{
    public class BookSeatViewModelTests
    {
        [Fact]
        public void Constructor_ShouldSetDefaults()
        {
            // Act
            var model = new BookSeatViewModel();

            // Assert
            Assert.False(model.IsBookingForSelf);
            Assert.Null(model.ExistingPassengers);
            Assert.Null(model.SelectedPassengerId);
        }

        [Fact]
        public void Can_SetAndGet_AllProperties()
        {
            // Arrange
            var model = new BookSeatViewModel
            {
                FlightId = 1,
                PassengerId = 2,
                FirstName = "Maria",
                FamilyName = "Ivanova",
                SelectedPassengerId = 3,
                ExistingPassengers = new List<SelectListItem> { new SelectListItem { Text = "Test", Value = "1" } },
                CreateNewPassenger = true,
                IsBookingForSelf = true,
                DepartureCity = "Sofia",
                ArrivalCity = "Varna",
                Duration = 60,
                Price = 99.99m
            };

            // Assert
            Assert.Equal(1, model.FlightId);
            Assert.Equal(2, model.PassengerId);
            Assert.Equal("Maria", model.FirstName);
            Assert.Equal("Ivanova", model.FamilyName);
            Assert.Equal(3, model.SelectedPassengerId);
            Assert.Single(model.ExistingPassengers);
            Assert.True(model.CreateNewPassenger);
            Assert.True(model.IsBookingForSelf);
            Assert.Equal("Sofia", model.DepartureCity);
            Assert.Equal("Varna", model.ArrivalCity);
            Assert.Equal(60, model.Duration);
            Assert.Equal(99.99m, model.Price);
        }
    }
}
