using Xunit;
using Airline_Ticket_System.Models.Account;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Airline_Ticket_System.Tests
{
    public class ProfileViewModelTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Fact]
        public void ProfileViewModel_ShouldBeValid_WithCorrectData()
        {
            // Arrange
            var model = new ProfileViewModel
            {
                FirstName = "Ivan",
                FamilyName = "Ivanov",
                Email = "ivan@example.com",
                Roles = new List<string> { "Admin", "User" }
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void ProfileViewModel_ShouldBeInvalid_WhenFirstNameIsEmpty()
        {
            var model = new ProfileViewModel
            {
                FirstName = "",
                FamilyName = "Ivanov",
                Email = "ivan@example.com",
                Roles = new List<string>()
            };

            var results = ValidateModel(model);
            Assert.Contains(results, r => r.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void ProfileViewModel_ShouldBeInvalid_WhenEmailIsInvalid()
        {
            var model = new ProfileViewModel
            {
                FirstName = "Ivan",
                FamilyName = "Ivanov",
                Email = "ivan-at-email", // невалиден email
                Roles = new List<string>()
            };

            var results = ValidateModel(model);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void ProfileViewModel_ShouldBeInvalid_WhenFamilyNameHasInvalidCharacters()
        {
            var model = new ProfileViewModel
            {
                FirstName = "Ivan",
                FamilyName = "Ivanov#", // забранен символ
                Email = "ivan@example.com",
                Roles = new List<string>()
            };

            var results = ValidateModel(model);
            Assert.Contains(results, r => r.MemberNames.Contains("FamilyName"));
        }
    }
}
