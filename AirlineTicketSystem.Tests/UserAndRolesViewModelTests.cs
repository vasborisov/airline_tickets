using Xunit;
using Airline_Ticket_System.Models;
using Airline_Ticket_System.Data.Entities;
using System.Collections.Generic;

namespace Airline_Ticket_System.Tests
{
    public class UserAndRolesViewModelTests
    {
        [Fact]
        public void Constructor_ShouldInitializeRolesAsEmptyList()
        {
            // Act
            var viewModel = new UserAndRolesViewModel();

            // Assert
            Assert.NotNull(viewModel.Roles);
            Assert.Empty(viewModel.Roles);
        }

        [Fact]
        public void UserAndRolesViewModel_ShouldStoreUserAndRoles()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Ivan",
                FamilyName = "Ivanov"
            };

            var roles = new List<string> { "Admin", "User" };

            // Act
            var viewModel = new UserAndRolesViewModel
            {
                User = user,
                Roles = roles
            };

            // Assert
            Assert.Equal("testuser", viewModel.User.UserName);
            Assert.Equal("Ivan", viewModel.User.FirstName);
            Assert.Equal(2, viewModel.Roles.Count);
            Assert.Contains("Admin", viewModel.Roles);
            Assert.Contains("User", viewModel.Roles);
        }
    }
}

