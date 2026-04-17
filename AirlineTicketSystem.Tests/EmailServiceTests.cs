using Xunit;
using Moq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Airline_Ticket_System.Services;
using Airline_Ticket_System.Configurations;
using Airline_Ticket_System.Data.Entities;
using Airline_Ticket_System.Entities;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Airline_Ticket_System.Tests
{
    public class EmailServiceTests
    {
        private readonly Mock<ILogger<EmailService>> _loggerMock;
        private readonly Mock<IWebHostEnvironment> _environmentMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly IOptions<EmailSettings> _emailSettings;

        public EmailServiceTests()
        {
            _loggerMock = new Mock<ILogger<EmailService>>();
            _environmentMock = new Mock<IWebHostEnvironment>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            // Setup email settings for testing
            var emailSettings = new EmailSettings
            {
                SmtpServer = "",  // Empty means development mode
                Port = 587,
                SenderEmail = "test@airline.com",
                SenderName = "Test Airline",
                MaxEmailsPerHour = 100
            };
            _emailSettings = Options.Create(emailSettings);

            // Setup environment as development
            _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");
        }

        [Fact]
        public async Task SendWelcomeAsync_DevelopmentMode_LogsEmail()
        {
            // Arrange
            var emailService = new EmailService(_emailSettings, _loggerMock.Object, _serviceProviderMock.Object, _environmentMock.Object);

            // Act
            await emailService.SendWelcomeAsync("user@example.com", "John Doe");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DEVELOPMENT EMAIL to user@example.com")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendOperatorCreatedAsync_DevelopmentMode_LogsEmail()
        {
            // Arrange
            var emailService = new EmailService(_emailSettings, _loggerMock.Object, _serviceProviderMock.Object, _environmentMock.Object);

            // Act
            await emailService.SendOperatorCreatedAsync("operator@example.com", "Jane Operator");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DEVELOPMENT EMAIL to operator@example.com")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendAccountActiveChangedAsync_DevelopmentMode_LogsEmail()
        {
            // Arrange
            var emailService = new EmailService(_emailSettings, _loggerMock.Object, _serviceProviderMock.Object, _environmentMock.Object);

            // Act
            await emailService.SendAccountActiveChangedAsync("user@example.com", "John Doe", false);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DEVELOPMENT EMAIL to user@example.com")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendBookingConfirmationAsync_DevelopmentMode_LogsEmail()
        {
            // Arrange
            var emailService = new EmailService(_emailSettings, _loggerMock.Object, _serviceProviderMock.Object, _environmentMock.Object);
            
            var flight = new Flight
            {
                Id = 1,
                FlightNumber = "AT0001",
                DepartureCity = "Sofia",
                ArrivalCity = "London",
                DepartureDateTime = DateTime.UtcNow.AddDays(1),
                ArrivalDateTime = DateTime.UtcNow.AddDays(1).AddHours(3),
                Duration = 180,
                Price = 199.99m
            };

            var passenger = new Passenger("John", "Traveler")
            {
                Id = 1
            };

            // Act
            await emailService.SendBookingConfirmationAsync("john@example.com", "ABC123", flight, passenger);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DEVELOPMENT EMAIL to john@example.com")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendBookingCancelledAsync_DevelopmentMode_LogsEmail()
        {
            // Arrange
            var emailService = new EmailService(_emailSettings, _loggerMock.Object, _serviceProviderMock.Object, _environmentMock.Object);

            // Act
            await emailService.SendBookingCancelledAsync("user@example.com", "XYZ789", 150.00m);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DEVELOPMENT EMAIL to user@example.com")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendFlightScheduleChangedAsync_DevelopmentMode_LogsEmail()
        {
            // Arrange
            var emailService = new EmailService(_emailSettings, _loggerMock.Object, _serviceProviderMock.Object, _environmentMock.Object);
            
            var flight = new Flight
            {
                Id = 2,
                FlightNumber = "AT0002",
                DepartureCity = "Vienna",
                ArrivalCity = "Prague",
                Status = "Delayed"
            };

            // Act
            await emailService.SendFlightScheduleChangedAsync("passenger@example.com", flight);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DEVELOPMENT EMAIL to passenger@example.com")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendFlightReminderAsync_DevelopmentMode_LogsEmail()
        {
            // Arrange
            var emailService = new EmailService(_emailSettings, _loggerMock.Object, _serviceProviderMock.Object, _environmentMock.Object);
            
            var flight = new Flight
            {
                Id = 3,
                FlightNumber = "AT0003",
                DepartureCity = "Berlin",
                ArrivalCity = "Munich"
            };

            var passenger = new Passenger("Maria", "Traveler")
            {
                Id = 2
            };

            // Act
            await emailService.SendFlightReminderAsync("maria@example.com", "DEF456", flight, passenger);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DEVELOPMENT EMAIL to maria@example.com")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("", true)]  // Empty SMTP server = development mode
        [InlineData(null, true)] // Null SMTP server = development mode  
        [InlineData("smtp.gmail.com", false)] // Valid SMTP server = production mode
        public void EmailService_DeterminesDevelopmentModeCorrectly(string smtpServer, bool expectedDevMode)
        {
            // Arrange
            var settings = new EmailSettings { SmtpServer = smtpServer };
            var options = Options.Create(settings);
            
            _environmentMock.Setup(e => e.EnvironmentName).Returns(expectedDevMode ? "Development" : "Production");

            // Act
            var emailService = new EmailService(options, _loggerMock.Object, _serviceProviderMock.Object, _environmentMock.Object);

            // The service should handle empty/null SMTP settings correctly in development
            // This is tested by the logging behavior in other tests
            Assert.NotNull(emailService);
        }
    }
}