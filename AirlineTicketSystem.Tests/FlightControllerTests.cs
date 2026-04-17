using Xunit;
using Moq;
using Airline_Ticket_System.Controllers;
using Airline_Ticket_System.Services.Interfaces;
using Airline_Ticket_System.Data.Entities;
using Airline_Ticket_System.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Airline_Ticket_System.Models.Flight;
using Airline_Ticket_System.Repositories;
using Microsoft.EntityFrameworkCore;
using Airline_Ticket_System.Repositories.Models;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using System;

namespace Airline_Ticket_System.Tests;
public class FlightControllerTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<FlightController>> _loggerMock;

    public FlightControllerTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<FlightController>>();
    }

    [Fact]
    public async Task Create_Post_ValidModel_AddsFlight_AndRedirects()
    {
        using var context = new ApplicationDbContext(_options);
        var mockService = new Mock<IFlightService>();
        var controller = new FlightController(context, mockService.Object, _emailServiceMock.Object, _loggerMock.Object);

        var model = new CreateFlightViewModel
        {
            Id = 1,
            DepartureCity = "London",
            ArrivalCity = "Berlin",
            Duration = 120,
            Price = 199.99M,
            Capacity = 10
        };

        var result = await controller.CreateAsync(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        mockService.Verify(s => s.AddFlightAsync(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public async Task Edit_Get_FlightExists_ReturnsViewWithModel()
    {
        using var context = new ApplicationDbContext(_options);
        var flight = new Flight(2, "Paris", "Rome", 90, 150.00M, 20);
        context.Flights.Add(flight);
        await context.SaveChangesAsync();

        var controller = new FlightController(context, Mock.Of<IFlightService>(), _emailServiceMock.Object, _loggerMock.Object);

        var result = await controller.EditAsync(2);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EditFlightViewModel>(view.Model);
        Assert.Equal("Paris", model.DepartureCity);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_UpdatesFlight_AndRedirects()
    {
        using var context = new ApplicationDbContext(_options);
        var flight = new Flight(3, "Madrid", "Lisbon", 80, 120.00M, 25)
        {
            FlightNumber = "FL003",
            DepartureDateTime = DateTime.UtcNow.AddDays(1),
            ArrivalDateTime = DateTime.UtcNow.AddDays(1).AddMinutes(80),
            Status = "Scheduled"
        };
        context.Flights.Add(flight);
        await context.SaveChangesAsync();

        var controller = new FlightController(context, Mock.Of<IFlightService>(), _emailServiceMock.Object, _loggerMock.Object);
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        var editModel = new EditFlightViewModel
        {
            Id = 3,
            DepartureCity = "Madrid",
            ArrivalCity = "Lisbon",
            Duration = 85,
            Price = 130.00M,
            Capacity = 30,
            FlightNumber = "FL003",
            DepartureDateTime = DateTime.UtcNow.AddDays(1),
            ArrivalDateTime = DateTime.UtcNow.AddDays(1).AddMinutes(85),
            Status = "Scheduled"
        };

        var result = await controller.EditAsync(editModel);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var updatedFlight = await context.Flights.FindAsync(3);
        Assert.Equal(85, updatedFlight.Duration);
        Assert.Equal(130.00M, updatedFlight.Price);
    }

    [Fact]
    public async Task Details_FlightExists_ReturnsViewWithModel()
    {
        using var context = new ApplicationDbContext(_options);
        var flight = new Flight(4, "Amsterdam", "Prague", 100, 110.00M, 15);
        context.Flights.Add(flight);
        await context.SaveChangesAsync();

        var controller = new FlightController(context, Mock.Of<IFlightService>(), _emailServiceMock.Object, _loggerMock.Object);

        var result = await controller.DetailsAsync(4);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<FlightViewModel>(view.Model);
        Assert.Equal("Amsterdam", model.DepartureCity);
    }

    [Fact]
    public async Task Delete_FlightExistsWithoutBookings_DeletesAndRedirects()
    {
        using var context = new ApplicationDbContext(_options);
        var flight = new Flight(5, "Vienna", "Zurich", 95, 180.00M, 12);
        context.Flights.Add(flight);
        await context.SaveChangesAsync();

        var mockService = new Mock<IFlightService>();
        mockService.Setup(s => s.DeleteFlightAsync(It.IsAny<Flight>())).Returns(Task.CompletedTask);

        var controller = new FlightController(context, mockService.Object, _emailServiceMock.Object, _loggerMock.Object);

        var result = await controller.DeleteAsync(5);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        mockService.Verify(s => s.DeleteFlightAsync(It.Is<Flight>(f => f.Id == 5)), Times.Once);
    }

    [Fact]
    public async Task Index_ReturnsFilteredFlights_WhenSearchIsProvided()
    {
        // Arrange
        var mockService = new Mock<IFlightService>();
        var searchViewModel = new FlightSearchViewModel
        {
            DepartureCity = "Sofia"
        };

        var flights = new List<Flight>
        {
            new Flight(1, "Sofia", "London", 180, 120.00m, 100),
            new Flight(2, "Plovdiv", "Paris", 200, 150.00m, 80)
        };

        // Свържи с празен списък за FlightPassengers, за да не хвърля null
        flights[0].FlightPassengers = new List<FlightPassenger>();
        flights[1].FlightPassengers = new List<FlightPassenger>();

        mockService.Setup(s => s.SearchFlightsAsync(It.Is<FlightSearchCriteria>(c => c.DepartureCity == "Sofia"), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((new List<Flight> { flights[0] }.AsReadOnly(), 1));
        
        mockService.Setup(s => s.SearchFlightsAsync(It.Is<FlightSearchCriteria>(c => c.DepartureCity != "Sofia"), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((flights.AsReadOnly(), flights.Count));

        var controller = new FlightController(null, mockService.Object, _emailServiceMock.Object, _loggerMock.Object);

        // Act
        var result = await controller.IndexAsync(searchViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<FlightIndexPageViewModel>(viewResult.Model);

        Assert.Single(model.Flights); // само един с "Sofia"
        Assert.Equal("Sofia", model.Flights[0].DepartureCity);
    }

    [Fact]
    public void BookSeat_Redirects_To_Booking_Create_With_FlightId()
    {
        var controller = new FlightController(null!, Mock.Of<IFlightService>(), _emailServiceMock.Object, _loggerMock.Object);

        var result = controller.BookSeat(7);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Create", redirect.ActionName);
        Assert.Equal("Booking", redirect.ControllerName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(7, redirect.RouteValues["id"]);
    }

    [Fact]
    public async Task Edit_Post_FlightWithPassengers_WithScheduleChange_SendsNotificationEmails()
    {
        using var context = new ApplicationDbContext(_options);
        
        // Create flight with passengers
        var flight = new Flight(10, "Vienna", "Prague", 120, 200.00m, 50)
        {
            DepartureDateTime = DateTime.UtcNow.AddDays(1),
            ArrivalDateTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            Status = "Scheduled",
            Gate = "A1"
        };
        context.Flights.Add(flight);

        var passenger = new Passenger("Maria", "Petrova");
        context.Passengers.Add(passenger);

        // Create ApplicationUser for the booking
        var user = new ApplicationUser
        {
            Id = "user-maria",
            UserName = "maria@example.com",
            Email = "maria@example.com",
            FirstName = "Maria",
            FamilyName = "Petrova"
        };
        context.Users.Add(user);

        var booking = new FlightPassenger
        {
            FlightId = flight.Id,
            PassengerId = passenger.Id,
            BookingStatus = "Confirmed",
            Pnr = "XYZ789",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = user.Id,
            CreatedByUser = user
        };
        context.FlightPassengers.Add(booking);

        await context.SaveChangesAsync();

        var controller = new FlightController(context, Mock.Of<IFlightService>(), _emailServiceMock.Object, _loggerMock.Object);
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        var editModel = new EditFlightViewModel
        {
            Id = 10,
            DepartureCity = "Vienna",
            ArrivalCity = "Prague", 
            Duration = 120,
            Price = 200.00m,
            Capacity = 50,
            FlightNumber = "AT0010",
            DepartureDateTime = DateTime.UtcNow.AddDays(1).AddHours(1), // Changed time
            ArrivalDateTime = DateTime.UtcNow.AddDays(1).AddHours(3),    // Changed time
            Status = "Delayed", // Changed status
            Gate = "B2" // Changed gate
        };

        var result = await controller.EditAsync(editModel);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        // Verify that flight schedule change email was sent to the passenger
        _emailServiceMock.Verify(x => x.SendFlightScheduleChangedAsync(
            "maria@example.com",
            It.Is<Flight>(f => f.Id == 10 && f.Status == "Delayed"),
            It.IsAny<CancellationToken>()), 
            Times.Once);

        // Verify success message includes notification count
        Assert.Contains("Notifications sent to 1 passengers", controller.TempData["SuccessMessage"].ToString());
    }

    [Fact]
    public async Task Edit_Post_FlightWithoutScheduleChange_DoesNotSendEmails()
    {
        using var context = new ApplicationDbContext(_options);
        
        var flight = new Flight(11, "Berlin", "Munich", 90, 150.00m, 40)
        {
            DepartureDateTime = DateTime.UtcNow.AddDays(2),
            ArrivalDateTime = DateTime.UtcNow.AddDays(2).AddHours(1.5),
            Status = "Scheduled",
            Gate = "C3",
            FlightNumber = "AT0011"
        };
        context.Flights.Add(flight);

        var passenger = new Passenger("Hans", "Mueller");
        context.Passengers.Add(passenger);

        // Create ApplicationUser for the booking (even though no email will be sent)
        var user = new ApplicationUser
        {
            Id = "user-hans",
            UserName = "hans@example.com",
            Email = "hans@example.com",
            FirstName = "Hans",
            FamilyName = "Mueller"
        };
        context.Users.Add(user);

        var booking = new FlightPassenger
        {
            FlightId = flight.Id,
            PassengerId = passenger.Id,
            BookingStatus = "Confirmed",
            Pnr = "DEF456",
            CreatedByUserId = user.Id,
            CreatedByUser = user
        };
        context.FlightPassengers.Add(booking);

        await context.SaveChangesAsync();

        var controller = new FlightController(context, Mock.Of<IFlightService>(), _emailServiceMock.Object, _loggerMock.Object);
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        var editModel = new EditFlightViewModel
        {
            Id = 11,
            DepartureCity = "Berlin",  // Same city
            ArrivalCity = "Munich",    // Same city
            Duration = 90,             // Same duration 
            Price = 150.00m,           // Same price
            Capacity = 40,             // Same capacity
            FlightNumber = "AT0011",   // Same flight number
            DepartureDateTime = flight.DepartureDateTime, // Same time
            ArrivalDateTime = flight.ArrivalDateTime,     // Same time
            Status = "Scheduled",      // Same status
            Gate = "C3"               // Same gate
        };

        var result = await controller.EditAsync(editModel);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        // Verify that NO email was sent (no schedule change)
        _emailServiceMock.Verify(x => x.SendFlightScheduleChangedAsync(
            It.IsAny<string>(),
            It.IsAny<Flight>(),
            It.IsAny<CancellationToken>()), 
            Times.Never);
    }

}

