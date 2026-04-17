using Airline_Ticket_System.Controllers;
using Airline_Ticket_System.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Airline_Ticket_System.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_Returns_ViewResult()
        {
            var controller = new HomeController();

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public void Error_Returns_SharedErrorView_With_RequestId()
        {
            var controller = new HomeController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var result = controller.Error();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Shared/Error.cshtml", viewResult.ViewName);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.False(string.IsNullOrEmpty(model.RequestId));
        }
    }
}


