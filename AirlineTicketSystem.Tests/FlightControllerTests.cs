using Xunit;
using Moq;
using Airline_Ticket_System.Controllers;
using Airline_Ticket_System.Services.Interfaces;
using Airline_Ticket_System.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Airline_Ticket_System.Models.Flight;
using Airline_Ticket_System.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Airline_Ticket_System.Tests;
public class FlightControllerTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public FlightControllerTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CreateAsync_ValidModel_AddsFlight_AndRedirects()
    {
        using var context = new ApplicationDbContext(_options);
        var mockService = new Mock<IFlightService>();
        var controller = new FlightController(context, mockService.Object);

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
        mockService.Verify(s => s.AddFlight(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public async Task Edit_Get_FlightExists_ReturnsViewWithModel()
    {
        using var context = new ApplicationDbContext(_options);
        var flight = new Flight(2, "Paris", "Rome", 90, 150.00M, 20);
        context.Flights.Add(flight);
        await context.SaveChangesAsync();

        var controller = new FlightController(context, Mock.Of<IFlightService>());

        var result = await controller.Edit(2);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<FlightViewModel>(view.Model);
        Assert.Equal("Paris", model.DepartureCity);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_UpdatesFlight_AndRedirects()
    {
        using var context = new ApplicationDbContext(_options);
        context.Flights.Add(new Flight(3, "Madrid", "Lisbon", 80, 120.00M, 25));
        await context.SaveChangesAsync();

        var controller = new FlightController(context, Mock.Of<IFlightService>());

        var editModel = new EditFlightViewModel
        {
            Id = 3,
            DepartureCity = "Madrid",
            ArrivalCity = "Lisbon",
            Duration = 85,
            Price = 130.00M,
            Capacity = 30
        };

        var result = await controller.Edit(editModel);

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

        var controller = new FlightController(context, Mock.Of<IFlightService>());

        var result = await controller.Details(4);

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

        var controller = new FlightController(context, mockService.Object);

        var result = await controller.Delete(5);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        mockService.Verify(s => s.DeleteFlightAsync(It.Is<Flight>(f => f.Id == 5)), Times.Once);
    }

    [Fact]
    public async Task Index_ReturnsFilteredFlights_WhenSearchIsProvided()
    {
        // Arrange
        var mockService = new Mock<IFlightService>();
        var departureCityFilter = "Sofia";

        var flights = new List<Flight>
        {
            new Flight(1, "Sofia", "London", 180, 120.00m, 100),
            new Flight(2, "Plovdiv", "Paris", 200, 150.00m, 80)
        };

        // Свържи с празен списък за FlightPassengers, за да не хвърля null
        flights[0].FlightPassengers = new List<FlightPassenger>();
        flights[1].FlightPassengers = new List<FlightPassenger>();

        mockService.Setup(s => s.LoadAllFlightsAsync())
                   .ReturnsAsync(flights);

        var controller = new FlightController(null, mockService.Object);

        // Act
        var result = await controller.Index(departureCityFilter);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Airline_Ticket_System.Models.Flight.FlightViewModel>>(viewResult.Model);

        Assert.Single(model); // само един с "Sofia"
        Assert.Equal("Sofia", model[0].DepartureCity);
    }

}

