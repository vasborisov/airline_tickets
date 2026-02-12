using Airline_Ticket_System.Controllers;
using Airline_Ticket_System.Data.Entities;
using Airline_Ticket_System.Entities;
using Airline_Ticket_System.Models.Booking;
using Airline_Ticket_System.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public BookingControllerTests()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "BookingTestDb")
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

            var controller = new BookingController(context, _userManagerMock.Object);
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

            var result = await controller.Create(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<BookSeatViewModel>(viewResult.Model);
            Assert.Equal("Ivan", model.FirstName);
            Assert.Equal("Petrov", model.FamilyName);
        }

        [Fact]
        public async Task Create_Post_ValidModel_NewPassenger_AddsBookingAndRedirects()
        {
            var context = new ApplicationDbContext(_dbContextOptions);
            await ClearDatabaseAsync(context);

            var flight = new Flight(1, "Sofia", "London", 180, 199.99m, 100);
            context.Flights.Add(flight);

            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "test@example.com",
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
                CreateNewPassenger = true
            };

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var controller = new BookingController(context, _userManagerMock.Object);
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

            var result = await controller.Create(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Flight", redirectResult.ControllerName);

            var booking = await context.FlightPassengers.FirstOrDefaultAsync();
            Assert.NotNull(booking);
            Assert.Equal(user.Id, booking.CreatedByUserId);

            var passenger = await context.Passengers.FirstOrDefaultAsync(p => p.FirstName == "Ivan" && p.FamilyName == "Petrov");
            Assert.NotNull(passenger);
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

            var controller = new BookingController(context, _userManagerMock.Object);

            var result = await controller.Create(model);

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

            var model = new BookSeatViewModel
            {
                FlightId = flight.Id,
                FirstName = "Ivan",
                FamilyName = "Petrov",
                CreateNewPassenger = false,
                SelectedPassengerId = passenger.Id
            };

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var controller = new BookingController(context, _userManagerMock.Object);

            var result = await controller.Create(model);

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

            var controller = new BookingController(context, _userManagerMock.Object);
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

            var result = await controller.MyBooked();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<FlightPassenger>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal(user.Id, model[0].CreatedByUserId);
        }

    }
}
