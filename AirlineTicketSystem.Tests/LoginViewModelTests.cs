using Xunit;
using Airline_Ticket_System.Models.Account;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Airline_Ticket_System.Tests
{
    public class LoginViewModelTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void LoginViewModel_Should_BeValid_WithValidData()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                RememberMe = true
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Empty(results); // няма грешки = моделът е валиден
        }

        [Fact]
        public void LoginViewModel_Should_BeInvalid_WhenEmailIsMissing()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "", // празно
                Password = "Password123!"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void LoginViewModel_Should_BeInvalid_WhenPasswordIsMissing()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "" // празно
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("Password"));
        }
    }
}