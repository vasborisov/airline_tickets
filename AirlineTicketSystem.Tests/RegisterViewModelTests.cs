using Xunit;
using Airline_Ticket_System.Models.Account;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Airline_Ticket_System.Tests
{
    public class RegisterViewModelTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Fact]
        public void RegisterViewModel_ShouldBeValid_WithCorrectData()
        {
            var model = new RegisterViewModel
            {
                FirstName = "Maria",
                FamilyName = "Petrova",
                Email = "maria@example.com",
                Password = "StrongPassword123",
                ConfirmPassword = "StrongPassword123"
            };

            var results = ValidateModel(model);
            Assert.Empty(results);
        }

        [Fact]
        public void RegisterViewModel_ShouldBeInvalid_WhenFirstNameIsMissing()
        {
            var model = new RegisterViewModel
            {
                FirstName = "",
                FamilyName = "Petrova",
                Email = "maria@example.com",
                Password = "123456",
                ConfirmPassword = "123456"
            };

            var results = ValidateModel(model);
            Assert.Contains(results, r => r.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void RegisterViewModel_ShouldBeInvalid_WhenEmailIsInvalid()
        {
            var model = new RegisterViewModel
            {
                FirstName = "Maria",
                FamilyName = "Petrova",
                Email = "not-an-email",
                Password = "123456",
                ConfirmPassword = "123456"
            };

            var results = ValidateModel(model);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void RegisterViewModel_ShouldBeInvalid_WhenPasswordsDoNotMatch()
        {
            var model = new RegisterViewModel
            {
                FirstName = "Maria",
                FamilyName = "Petrova",
                Email = "maria@example.com",
                Password = "123456",
                ConfirmPassword = "differentPassword"
            };

            var results = ValidateModel(model);
            Assert.Contains(results, r => r.ErrorMessage.Contains("Passwords do not match."));
        }

        [Fact]
        public void RegisterViewModel_ShouldBeInvalid_WhenFamilyNameHasInvalidCharacters()
        {
            var model = new RegisterViewModel
            {
                FirstName = "Maria",
                FamilyName = "Petrova#", // Грешен символ
                Email = "maria@example.com",
                Password = "123456",
                ConfirmPassword = "123456"
            };

            var results = ValidateModel(model);
            Assert.Contains(results, r => r.MemberNames.Contains("FamilyName"));
        }
    }
}

