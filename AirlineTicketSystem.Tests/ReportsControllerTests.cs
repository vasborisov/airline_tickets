using Airline_Ticket_System.Controllers;
using Airline_Ticket_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Airline_Ticket_System.Tests
{
    public class ReportsControllerTests
    {
        private readonly Mock<IReportService> _reportServiceMock;
        private readonly ReportsController _controller;

        public ReportsControllerTests()
        {
            _reportServiceMock = new Mock<IReportService>();
            _controller = new ReportsController(_reportServiceMock.Object);
        }

        [Fact]
        public void DailyFlights_HasCorrectAuthorization()
        {
            // Arrange
            var method = typeof(ReportsController).GetMethod("DailyFlights");

            // Act
            var authorizeAttribute = method.GetCustomAttribute<AuthorizeAttribute>();

            // Assert
            Assert.NotNull(authorizeAttribute);
            Assert.Equal("Admin,Operator", authorizeAttribute.Roles);
        }

        [Fact]
        public void BookingStatistics_HasCorrectAuthorization()
        {
            // Arrange
            var method = typeof(ReportsController).GetMethod("BookingStatistics");

            // Act
            var authorizeAttribute = method.GetCustomAttribute<AuthorizeAttribute>();

            // Assert
            Assert.NotNull(authorizeAttribute);
            Assert.Equal("Admin,Operator", authorizeAttribute.Roles);
        }

        [Fact]
        public void Financial_HasAdminOnlyAuthorization()
        {
            // Arrange
            var method = typeof(ReportsController).GetMethod("Financial");

            // Act
            var authorizeAttribute = method.GetCustomAttribute<AuthorizeAttribute>();

            // Assert
            Assert.NotNull(authorizeAttribute);
            Assert.Equal("Admin", authorizeAttribute.Roles);
        }

        [Fact]
        public void ReportsController_HasBaseAuthorizeAttribute()
        {
            // Arrange
            var controllerType = typeof(ReportsController);

            // Act
            var authorizeAttribute = controllerType.GetCustomAttribute<AuthorizeAttribute>();

            // Assert
            Assert.NotNull(authorizeAttribute);
            Assert.Null(authorizeAttribute.Roles); // Base authorize should not specify roles
        }

        [Fact]
        public async Task DailyFlights_CallsReportService()
        {
            // Arrange
            var testDate = DateTime.Today;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Role, "Admin")
                    }))
                }
            };

            // Act
            await _controller.DailyFlights(testDate);

            // Assert
            _reportServiceMock.Verify(r => r.GetDailyFlightsAsync(testDate, default), Times.Once);
        }

        [Fact]
        public async Task BookingStatistics_CallsReportService()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Role, "Operator")
                    }))
                }
            };

            // Act
            await _controller.BookingStatistics();

            // Assert
            _reportServiceMock.Verify(r => r.GetBookingStatisticsAsync(default), Times.Once);
        }

        [Fact]
        public async Task Financial_CallsReportService()
        {
            // Arrange
            var fromDate = DateTime.Today.AddDays(-30);
            var toDate = DateTime.Today;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Role, "Admin")
                    }))
                }
            };

            // Act
            await _controller.Financial(fromDate, toDate);

            // Assert
            _reportServiceMock.Verify(r => r.GetFinancialReportAsync(
                It.IsAny<DateTime?>(), 
                It.IsAny<DateTime?>(), default), Times.Once);
        }
    }
}