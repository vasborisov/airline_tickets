using Xunit;
using Airline_Ticket_System.Data.Entities;

namespace Airline_Ticket_System.Tests
{
    public class ApplicationUserTests
    {
        [Fact]
        public void ApplicationUser_ShouldSetAndGetFirstNameAndFamilyName()
        {
            // Arrange
            var user = new ApplicationUser
            {
                FirstName = "Ivan",
                FamilyName = "Ivanov"
            };

            // Act
            var firstName = user.FirstName;
            var familyName = user.FamilyName;

            // Assert
            Assert.Equal("Ivan", firstName);
            Assert.Equal("Ivanov", familyName);
        }
    }
}

