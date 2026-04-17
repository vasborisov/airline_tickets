using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Airline_Ticket_System.Services;
using Airline_Ticket_System.Services.Interfaces;
using Airline_Ticket_System.Repositories;
using Airline_Ticket_System.Data.Entities;
using Airline_Ticket_System.Entities;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Airline_Ticket_System.Tests
{
    public class FlightReminderBackgroundServiceTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<ILogger<FlightReminderBackgroundService>> _loggerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public FlightReminderBackgroundServiceTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _loggerMock = new Mock<ILogger<FlightReminderBackgroundService>>();
            _emailServiceMock = new Mock<IEmailService>();
            
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task SendFlightRemindersAsync_FindsFlightsIn24HourWindow()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            
            // Create a flight departing in 24.5 hours (should be included)
            var targetFlight = new Flight
            {
                Id = 1,
                FlightNumber = "AT0001",
                DepartureCity = "Sofia",
                ArrivalCity = "London",
                DepartureDateTime = DateTime.UtcNow.AddHours(24.5),
                Status = "Scheduled"
            };
            
            // Create a flight departing in 26 hours (should be excluded - outside window)
            var outsideFlight = new Flight
            {
                Id = 2,
                FlightNumber = "AT0002", 
                DepartureCity = "Vienna",
                ArrivalCity = "Prague",
                DepartureDateTime = DateTime.UtcNow.AddHours(26),
                Status = "Scheduled"
            };

            var passenger = new Passenger("John", "Traveler")
            {
                Id = 1
            };

            // Create an ApplicationUser for the booking
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "john@example.com",
                Email = "john@example.com",
                FirstName = "John",
                FamilyName = "Traveler"
            };

            var confirmedBooking = new FlightPassenger
            {
                Id = 1,
                FlightId = targetFlight.Id,
                PassengerId = passenger.Id,
                BookingStatus = "Confirmed",
                Pnr = "ABC123",
                CreatedByUserId = user.Id,
                CreatedByUser = user
            };

            context.Flights.AddRange(targetFlight, outsideFlight);
            context.Passengers.Add(passenger);
            context.Users.Add(user);
            context.FlightPassengers.Add(confirmedBooking);
            await context.SaveChangesAsync();

            // Setup service provider mock
            var scopeMock = new Mock<IServiceScope>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            
            serviceProviderMock.Setup(p => p.GetService(typeof(ApplicationDbContext)))
                              .Returns(context);
            serviceProviderMock.Setup(p => p.GetService(typeof(IEmailService)))
                              .Returns(_emailServiceMock.Object);
            
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
            _serviceProviderMock.Setup(p => p.GetService(typeof(IServiceScopeFactory)))
                               .Returns(scopeFactoryMock.Object);

            var backgroundService = new FlightReminderBackgroundService(_serviceProviderMock.Object, _loggerMock.Object);

            // Use reflection to access the private method
            var sendRemindersMethod = typeof(FlightReminderBackgroundService)
                .GetMethod("SendFlightRemindersAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            await (Task)sendRemindersMethod.Invoke(backgroundService, new object[] { CancellationToken.None });

            // Assert
            // Verify email was sent for the target flight only
            _emailServiceMock.Verify(e => e.SendFlightReminderAsync(
                "john@example.com",
                "ABC123",
                It.Is<Flight>(f => f.FlightNumber == "AT0001"),
                It.Is<Passenger>(p => p.FirstName == "John" && p.FamilyName == "Traveler"),
                It.IsAny<CancellationToken>()), Times.Once);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found 1 flights requiring reminder emails")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendFlightRemindersAsync_OnlyProcessesConfirmedBookings()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            
            var flight = new Flight
            {
                Id = 3,
                FlightNumber = "AT0003",
                DepartureCity = "Berlin",
                ArrivalCity = "Munich",
                DepartureDateTime = DateTime.UtcNow.AddHours(24.2),
                Status = "Scheduled"
            };

            var passenger1 = new Passenger("Maria", "Confirmed")
            {
                Id = 2
            };

            var passenger2 = new Passenger("Hans", "Cancelled")
            {
                Id = 3
            };

            // Create ApplicationUser for the confirmed booking
            var user = new ApplicationUser
            {
                Id = "user2",
                UserName = "maria@example.com",
                Email = "maria@example.com",
                FirstName = "Maria",
                FamilyName = "Confirmed"
            };

            var confirmedBooking = new FlightPassenger
            {
                Id = 2,
                FlightId = flight.Id,
                PassengerId = passenger1.Id,
                BookingStatus = "Confirmed",
                Pnr = "DEF456",
                CreatedByUserId = user.Id,
                CreatedByUser = user
            };

            var cancelledBooking = new FlightPassenger
            {
                Id = 3,
                FlightId = flight.Id,
                PassengerId = passenger2.Id,
                BookingStatus = "Cancelled",
                Pnr = "GHI789"
            };

            context.Flights.Add(flight);
            context.Passengers.AddRange(passenger1, passenger2);
            context.Users.Add(user);
            context.FlightPassengers.AddRange(confirmedBooking, cancelledBooking);
            await context.SaveChangesAsync();

            // Setup service provider mock
            var scopeMock = new Mock<IServiceScope>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            
            serviceProviderMock.Setup(p => p.GetService(typeof(ApplicationDbContext)))
                              .Returns(context);
            serviceProviderMock.Setup(p => p.GetService(typeof(IEmailService)))
                              .Returns(_emailServiceMock.Object);
            
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
            _serviceProviderMock.Setup(p => p.GetService(typeof(IServiceScopeFactory)))
                               .Returns(scopeFactoryMock.Object);

            var backgroundService = new FlightReminderBackgroundService(_serviceProviderMock.Object, _loggerMock.Object);

            // Use reflection to access the private method
            var sendRemindersMethod = typeof(FlightReminderBackgroundService)
                .GetMethod("SendFlightRemindersAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            await (Task)sendRemindersMethod.Invoke(backgroundService, new object[] { CancellationToken.None });

            // Assert
            // Verify email was sent only to confirmed booking passenger
            _emailServiceMock.Verify(e => e.SendFlightReminderAsync(
                "maria@example.com",
                "DEF456",
                It.IsAny<Flight>(),
                It.Is<Passenger>(p => p.FirstName == "Maria" && p.FamilyName == "Confirmed"),
                It.IsAny<CancellationToken>()), Times.Once);

            // Verify no email was sent to cancelled booking passenger
            _emailServiceMock.Verify(e => e.SendFlightReminderAsync(
                "hans@example.com",
                "GHI789",
                It.IsAny<Flight>(),
                It.Is<Passenger>(p => p.FirstName == "Hans" && p.FamilyName == "Cancelled"),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SendFlightRemindersAsync_HandlesEmailServiceError()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            
            var flight = new Flight
            {
                Id = 4,
                FlightNumber = "AT0004",
                DepartureCity = "Paris",
                ArrivalCity = "Rome",
                DepartureDateTime = DateTime.UtcNow.AddHours(24.1),
                Status = "Scheduled"
            };

            var passenger = new Passenger("Pierre", "Traveler")
            {
                Id = 4
            };

            // Create ApplicationUser for the booking
            var user = new ApplicationUser
            {
                Id = "user4",
                UserName = "pierre@example.com",
                Email = "pierre@example.com",
                FirstName = "Pierre",
                FamilyName = "Traveler"
            };

            var booking = new FlightPassenger
            {
                Id = 4,
                FlightId = flight.Id,
                PassengerId = passenger.Id,
                BookingStatus = "Confirmed",
                Pnr = "JKL012",
                CreatedByUserId = user.Id,
                CreatedByUser = user
            };

            context.Flights.Add(flight);
            context.Passengers.Add(passenger);
            context.Users.Add(user);
            context.FlightPassengers.Add(booking);
            await context.SaveChangesAsync();

            // Setup email service to throw exception
            _emailServiceMock.Setup(e => e.SendFlightReminderAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Flight>(),
                It.IsAny<Passenger>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("SMTP connection failed"));

            // Setup service provider mock
            var scopeMock = new Mock<IServiceScope>();
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            
            serviceProviderMock.Setup(p => p.GetService(typeof(ApplicationDbContext)))
                              .Returns(context);
            serviceProviderMock.Setup(p => p.GetService(typeof(IEmailService)))
                              .Returns(_emailServiceMock.Object);
            
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
            _serviceProviderMock.Setup(p => p.GetService(typeof(IServiceScopeFactory)))
                               .Returns(scopeFactoryMock.Object);

            var backgroundService = new FlightReminderBackgroundService(_serviceProviderMock.Object, _loggerMock.Object);

            // Use reflection to access the private method
            var sendRemindersMethod = typeof(FlightReminderBackgroundService)
                .GetMethod("SendFlightRemindersAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act & Assert (should not throw)
            await (Task)sendRemindersMethod.Invoke(backgroundService, new object[] { CancellationToken.None });

            // Verify error was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send flight reminder")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}