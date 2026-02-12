using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Airline_Ticket_System.Controllers;

namespace Airline_Ticket_System.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_Returns_ViewResult()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);  // Проверява дали резултатът е ViewResult
            Assert.Null(viewResult.ViewName);  // Ако името на изгледа не е зададено, трябва да е null
        }
    }
}


