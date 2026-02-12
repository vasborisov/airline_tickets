using Airline_Ticket_System.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Airline_Ticket_System.Tests
{
    public class FlightModelTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);
            return validationResults;
        }

        [Fact]
        public void Flight_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var flight = new Flight(1, "Sofia", "London", 90, 199.99m, 150);

            // Act
            var results = ValidateModel(flight);

            // Assert
            Assert.Empty(results); // няма грешки -> моделът е валиден
        }

        [Fact]
        public void Flight_WithShortDuration_ShouldFailValidation()
        {
            // Arrange
            var flight = new Flight(1, "Sofia", "London", 20, 199.99m, 150);

            // Act
            var results = ValidateModel(flight);

            // Assert
            Assert.Contains(results, v => v.ErrorMessage.Contains("Duration must be at least 30 minutes."));
        }

        [Fact]
        public void Flight_MissingRequiredFields_ShouldFailValidation()
        {
            // Arrange – задаваме празни/некоректни стойности
            var flight = new Flight(1, null!, null!, 90, 199.99m, 150);

            // Act
            var results = ValidateModel(flight);

            // Assert
            Assert.Contains(results, v => v.MemberNames.Contains("DepartureCity"));
            Assert.Contains(results, v => v.MemberNames.Contains("ArrivalCity"));
        }
    }
}

