using Airline_Ticket_System.Controllers;
using Airline_Ticket_System.Data.Entities;
using Airline_Ticket_System.Entities;
using Airline_Ticket_System.Models.Booking;
using Airline_Ticket_System.Repositories;
using Airline_Ticket_System.Services;
using Airline_Ticket_System.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Airline_Ticket_System.Tests
{
    public class BookingControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<BookingController>> _loggerMock;
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public BookingControllerTests()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<BookingController>>();

            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "BookingTestDb")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
        }

        private async Task ClearDatabaseAsync(ApplicationDbContext context)
        {
            context.Database.EnsureDeleted(); // Deletes the in-memory database
            context.Database.EnsureCreated(); // Re-creates the database schema
            await Task.CompletedTask;
        }

        [Fact]
        public async Task Create_Get_FlightExists_UserInRole_UserDetailsPopulated()
        {
            var context = new ApplicationDbContext(_dbContextOptions);
            await ClearDatabaseAsync(context);

            var flight = new Flight(1, "Sofia", "London", 180, 199.99m, 100);

            if (!await context.Flights.AnyAsync(f => f.Id == flight.Id))
            {
                context.Flights.Add(flight);
                await context.SaveChangesAsync();
            }

            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "test@example.com",
                FirstName = "Ivan",
                FamilyName = "Petrov"
            };

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var controller = new BookingController(context, _userManagerMock.Object, new BookingService(context), _emailServiceMock.Object, _loggerMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Role, "User")
                    }))
                }
            };

            var result = await controller.CreateAsync(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<BookSeatViewModel>(viewResult.Model);
            Assert.Equal("Ivan", model.FirstName);
            Assert.Equal("Petrov", model.FamilyName);
        }

        [Fact]
        public async Task Create_Post_ValidModel_NewPassenger_UserRole_RedirectsToMyBooked()
        {
            var context = new ApplicationDbContext(_dbContextOptions);
            await ClearDatabaseAsync(context);

            var flight = new Flight(1, "Sofia", "London", 180, 199.99m, 100);
            context.Flights.Add(flight);

            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "test@example.com",
                Email = "test@example.com",
                FirstName = "Ivan", 
                FamilyName = "Petrov"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var model = new BookSeatViewModel
            {
                FlightId = flight.Id,
                FirstName = "Ivan",
                FamilyName = "Petrov",
                Email = "test@example.com",
                CreateNewPassenger = true
            };

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var controller = new BookingController(context, _userManagerMock.Object, new BookingService(context), _emailServiceMock.Object, _loggerMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Role, "User")  // Explicitly set User role
                    }))
                }
            };

            // Initialize TempData
            controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());

            var result = await controller.CreateAsync(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("MyBooked", redirectResult.ActionName);
            Assert.Equal("Booking", redirectResult.ControllerName);

            var booking = await context.FlightPassengers.FirstOrDefaultAsync();
            Assert.NotNull(booking);
            Assert.Equal(user.Id, booking.CreatedByUserId);

            var passenger = await context.Passengers.FirstOrDefaultAsync(p => p.FirstName == "Ivan" && p.FamilyName == "Petrov");
            Assert.NotNull(passenger);

            // Verify that booking confirmation email was sent
            _emailServiceMock.Verify(x => x.SendBookingConfirmationAsync(
                user.Email,
                It.IsAny<string>(), // PNR
                It.IsAny<Flight>(),
                It.IsAny<Passenger>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task Create_Post_ValidModel_AdminRole_RedirectsToFlights()
        {
            var context = new ApplicationDbContext(_dbContextOptions);
            await ClearDatabaseAsync(context);

            var flight = new Flight(1, "Sofia", "London", 180, 199.99m, 100);
            context.Flights.Add(flight);

            var admin = new ApplicationUser
            {
                Id = "admin123",
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FirstName = "Admin", 
                FamilyName = "User"
            };
            context.Users.Add(admin);
            await context.SaveChangesAsync();

            var model = new BookSeatViewModel
            {
                FlightId = flight.Id,
                FirstName = "John",
                FamilyName = "Passenger",
                Email = "john@example.com",
                CreateNewPassenger = true
            };

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(admin);

            var controller = new BookingController(context, _userManagerMock.Object, new BookingService(context), _emailServiceMock.Object, _loggerMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, admin.Id),
                        new Claim(ClaimTypes.Role, "Admin")  // Admin role
                    }))
                }
            };

            // Initialize TempData
            controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());

            var result = await controller.CreateAsync(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Flight", redirectResult.ControllerName);

            var booking = await context.FlightPassengers.FirstOrDefaultAsync();
            Assert.NotNull(booking);
            Assert.Equal(admin.Id, booking.CreatedByUserId);
        }

        [Fact]
        public async Task Create_Post_ValidModel_OperatorRole_RedirectsToFlights()
        {
            var context = new ApplicationDbContext(_dbContextOptions);
            await ClearDatabaseAsync(context);

            var flight = new Flight(1, "Sofia", "London", 180, 199.99m, 100);
            context.Flights.Add(flight);

            var operatorUser = new ApplicationUser
            {
                Id = "op123",
                UserName = "operator@example.com",
                Email = "operator@example.com",
                FirstName = "Operator", 
                FamilyName = "User"
            };
            context.Users.Add(operatorUser);
            await context.SaveChangesAsync();

            var model = new BookSeatViewModel
            {
                FlightId = flight.Id,
                FirstName = "Jane",
                FamilyName = "Passenger",
                Email = "jane@example.com", 
                CreateNewPassenger = true
            };

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(operatorUser);

            var controller = new BookingController(context, _userManagerMock.Object, new BookingService(context), _emailServiceMock.Object, _loggerMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, operatorUser.Id),
                        new Claim(ClaimTypes.Role, "Operator")  // Operator role
                    }))
                }
            };

            // Initialize TempData
            controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());

            var result = await controller.CreateAsync(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Flight", redirectResult.ControllerName);

            var booking = await context.FlightPassengers.FirstOrDefaultAsync();
            Assert.NotNull(booking);
            Assert.Equal(operatorUser.Id, booking.CreatedByUserId);
        }



        [Fact]
        public async Task Create_Post_FlightDoesNotExist_ReturnsError()
        {
            var context = new ApplicationDbContext(_dbContextOptions);
            await ClearDatabaseAsync(context);

            var model = new BookSeatViewModel
            {
                FlightId = 999, // Non-existing flight ID
                FirstName = "Ivan",
                FamilyName = "Petrov"
            };

            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "test@example.com"
            };

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var controller = new BookingController(context, _userManagerMock.Object, new BookingService(context), _emailServiceMock.Object, _loggerMock.Object);

            var result = await controller.CreateAsync(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("A flight with the provided id does not exist", controller.ModelState[""].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Create_Post_AlreadyBooked_ReturnsError()
        {
            var context = new ApplicationDbContext(_dbContextOptions);
            await ClearDatabaseAsync(context);

            var flight = new Flight(1, "Sofia", "London", 180, 199.99m, 100);
            context.Flights.Add(flight);
            await context.SaveChangesAsync();

            var passenger = new Passenger("Ivan", "Petrov");
            context.Passengers.Add(passenger);
            await context.SaveChangesAsync();

            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "test@example.com"
            };

            var booking = new FlightPassenger
            {
                FlightId = flight.Id,
                PassengerId = passenger.Id,
                CreatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.FlightPassengers.Add(booking);
            await context.SaveChangesAsync();

            // Add the user to context so it can be found
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var model = new BookSeatViewModel
            {
                FlightId = flight.Id,
                FirstName = "Ivan",
                FamilyName = "Petrov",
                CreateNewPassenger = false,
                SelectedPassengerId = user.Id
            };

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var controller = new BookingController(context, _userManagerMock.Object, new BookingService(context), _emailServiceMock.Object, _loggerMock.Object);

            var result = await controller.CreateAsync(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("This passenger has already booked this flight.", controller.ModelState[""].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task MyBooked_ReturnsUserBookings()
        {
            var context = new ApplicationDbContext(_dbContextOptions);
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "test@example.com",
                FirstName = "Test",
                FamilyName = "User"
            };
            context.Users.Add(user);

            var passenger = new Passenger("Ivan", "Petrov");
            context.Passengers.Add(passenger);

            var flight = new Flight(1, "Sofia", "London", 180, 199.99m, 100);
            context.Flights.Add(flight);

            var booking = new FlightPassenger
            {
                FlightId = flight.Id,
                PassengerId = passenger.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = user.Id
            };
            context.FlightPassengers.Add(booking);

            await context.SaveChangesAsync();

            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id);

            var controller = new BookingController(context, _userManagerMock.Object, new BookingService(context), _emailServiceMock.Object, _loggerMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
                    }))
                }
            };

            var result = await controller.MyBookedAsync();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<FlightPassenger>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal(user.Id, model[0].CreatedByUserId);
        }

        [Fact]
        public async Task Cancel_ValidBooking_SendsCancellationEmail()
        {
            var context = new ApplicationDbContext(_dbContextOptions);
            await ClearDatabaseAsync(context);

            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "test@example.com",
                Email = "test@example.com",
                FirstName = "Test",
                FamilyName = "User"
            };
            context.Users.Add(user);

            var flight = new Flight(1, "Sofia", "London", 180, 199.99m, 100);
            context.Flights.Add(flight);

            var passenger = new Passenger("Ivan", "Petrov");
            context.Passengers.Add(passenger);

            var booking = new FlightPassenger
            {
                FlightId = flight.Id,
                PassengerId = passenger.Id,
                CreatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                BookingStatus = "Confirmed",
                Pnr = "ABC123",
                PaymentAmount = 199.99m
            };
            context.FlightPassengers.Add(booking);
            await context.SaveChangesAsync();

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var controller = new BookingController(context, _userManagerMock.Object, new BookingService(context), _emailServiceMock.Object, _loggerMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id)
                    }))
                }
            };
            controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());

            var result = await controller.CancelAsync(booking.Id);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("MyBooked", redirectResult.ActionName);

            // Verify that booking cancellation email was sent
            _emailServiceMock.Verify(x => x.SendBookingCancelledAsync(
                user.Email,
                "ABC123",
                It.IsAny<decimal?>(),
                It.IsAny<Flight?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task Create_Post_EmailFailure_BookingSucceedsWithWarning()
        {
            var context = new ApplicationDbContext(_dbContextOptions);
            await ClearDatabaseAsync(context);

            var flight = new Flight(1, "Sofia", "London", 180, 199.99m, 100);
            context.Flights.Add(flight);

            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "test@example.com",
                Email = "test@example.com",
                FirstName = "Ivan", 
                FamilyName = "Petrov"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var model = new BookSeatViewModel
            {
                FlightId = flight.Id,
                FirstName = "Ivan",
                FamilyName = "Petrov",
                Email = "test@example.com",
                CreateNewPassenger = true
            };

            // Setup email service to throw exception
            _emailServiceMock.Setup(x => x.SendBookingConfirmationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Flight>(), It.IsAny<Passenger>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("SMTP server unavailable"));

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var controller = new BookingController(context, _userManagerMock.Object, new BookingService(context), _emailServiceMock.Object, _loggerMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Role, "User")  // Explicitly set User role for expected redirect
                    }))
                }
            };

            // Initialize TempData
            controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());

            var result = await controller.CreateAsync(model);

            // Should still redirect to success page despite email failure
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("MyBooked", redirectResult.ActionName);
            Assert.Equal("Booking", redirectResult.ControllerName);

            // Verify booking was created in database
            var booking = await context.FlightPassengers.FirstOrDefaultAsync();
            Assert.NotNull(booking);
            Assert.Equal(user.Id, booking.CreatedByUserId);

            // Verify passenger was created
            var passenger = await context.Passengers.FirstOrDefaultAsync(p => p.FirstName == "Ivan" && p.FamilyName == "Petrov");
            Assert.NotNull(passenger);

            // Verify email sending was attempted
            _emailServiceMock.Verify(x => x.SendBookingConfirmationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Flight>(), It.IsAny<Passenger>(), It.IsAny<CancellationToken>()), 
                Times.Once);

            // Verify error was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send booking confirmation email")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify TempData contains email warning
            Assert.True(controller.TempData.ContainsKey("EmailWarning"));
            Assert.Contains("couldn't send the confirmation email", controller.TempData["EmailWarning"].ToString());
        }

        [Fact]
        public async Task Cancel_EmailFailure_CancellationSucceedsWithWarning()
        {
            var context = new ApplicationDbContext(_dbContextOptions);
            await ClearDatabaseAsync(context);

            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "test@example.com",
                Email = "test@example.com",
                FirstName = "Test",
                FamilyName = "User"
            };
            context.Users.Add(user);

            var flight = new Flight(1, "Sofia", "London", 180, 199.99m, 100);
            context.Flights.Add(flight);

            var passenger = new Passenger("Ivan", "Petrov");
            context.Passengers.Add(passenger);

            var booking = new FlightPassenger
            {
                FlightId = flight.Id,
                PassengerId = passenger.Id,
                CreatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                BookingStatus = "Confirmed",
                Pnr = "ABC123",
                PaymentAmount = 199.99m
            };
            context.FlightPassengers.Add(booking);
            await context.SaveChangesAsync();

            // Setup email service to throw exception
            _emailServiceMock.Setup(x => x.SendBookingCancelledAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<Flight?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Email service unavailable"));

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var controller = new BookingController(context, _userManagerMock.Object, new BookingService(context), _emailServiceMock.Object, _loggerMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id)
                    }))
                }
            };
            controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());

            var result = await controller.CancelAsync(booking.Id);

            // Should still redirect to success page despite email failure
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("MyBooked", redirectResult.ActionName);

            // Verify email sending was attempted
            _emailServiceMock.Verify(x => x.SendBookingCancelledAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<Flight?>(), It.IsAny<CancellationToken>()), 
                Times.Once);

            // Verify error was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send booking cancellation email")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify TempData contains email warning
            Assert.True(controller.TempData.ContainsKey("EmailWarning"));
            Assert.Contains("couldn't send the cancellation email", controller.TempData["EmailWarning"].ToString());
        }

        [Fact]
        public async Task Cancel_ValidBooking_SendsCancellationEmailWithFlightDetails()
        {
            var context = new ApplicationDbContext(_dbContextOptions);
            await ClearDatabaseAsync(context);

            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "test@example.com",
                Email = "test@example.com",
                FirstName = "Test",
                FamilyName = "User"
            };
            context.Users.Add(user);

            var flight = new Flight(1, "Sofia", "London", 180, 199.99m, 100);
            context.Flights.Add(flight);

            var passenger = new Passenger("Ivan", "Petrov");
            context.Passengers.Add(passenger);

            var booking = new FlightPassenger
            {
                FlightId = flight.Id,
                PassengerId = passenger.Id,
                CreatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                BookingStatus = "Confirmed",
                Pnr = "DEF456",
                PaymentAmount = 199.99m
            };
            context.FlightPassengers.Add(booking);
            await context.SaveChangesAsync();

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var controller = new BookingController(context, _userManagerMock.Object, new BookingService(context), _emailServiceMock.Object, _loggerMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id)
                    }))
                }
            };
            controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());

            var result = await controller.CancelAsync(booking.Id);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("MyBooked", redirectResult.ActionName);

            // Verify that booking cancellation email was sent with flight details
            _emailServiceMock.Verify(x => x.SendBookingCancelledAsync(
                user.Email,
                "DEF456",
                It.IsAny<decimal?>(),
                It.Is<Flight?>(f => f != null && f.FlightNumber == "AT0001" && f.DepartureCity == "Sofia"),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

    }
}
