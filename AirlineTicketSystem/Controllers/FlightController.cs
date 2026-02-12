using Airline_Ticket_System.Entities;
using Airline_Ticket_System.Models;
using Airline_Ticket_System.Models.Booking;
using Airline_Ticket_System.Models.Flight;
using Airline_Ticket_System.Models.Passenger;
using Airline_Ticket_System.Repositories;
using Airline_Ticket_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Airline_Ticket_System.Controllers
{
    public class FlightController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFlightService _flightService;

        public FlightController(ApplicationDbContext context, IFlightService flightService)
        {
            _context = context;
            _flightService = flightService;
        }

        public async Task<IActionResult> Index(string searchDepartureCity)
        {
            var flights = await _flightService.LoadAllFlightsAsync();

            // Apply filtering if the searchDepartureCity is provided
            if (!String.IsNullOrEmpty(searchDepartureCity))
            {
                flights = flights.Where(f => f.DepartureCity.Equals(searchDepartureCity, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var viewModels = flights.Select(f => new FlightViewModel(
                                                        f.Id,
                                                        f.DepartureCity,
                                                        f.ArrivalCity,
                                                        f.Duration,
                                                        f.Price,
                                                        f.Capacity,
                                                        f.Capacity <= 0,
                                                        f.FlightPassengers.Select(fp => new PassengerViewModel(fp.Passenger.Id, fp.Passenger.FirstName, fp.Passenger.FamilyName)).ToList()
            )).ToList();

            // Return the filtered list to the view
            return View(viewModels);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync(CreateFlightViewModel flightViewModel)
        {

            if (!ModelState.IsValid) {
                return View(flightViewModel);
            } 

            var newFlightEntity = new Flight(
                flightViewModel.Id,
                flightViewModel.DepartureCity,
                flightViewModel.ArrivalCity,
                flightViewModel.Duration,
                flightViewModel.Price,
                flightViewModel.Capacity);

            await _flightService.AddFlight(newFlightEntity);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]

        public async Task<IActionResult> Edit(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.FlightPassengers)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flight == null)
            {
                // If the flight doesn't exist
                ModelState.AddModelError(string.Empty, "A flight with the provided id does not exist");
                return View();
            }

            if (flight.FlightPassengers != null && flight.FlightPassengers.Any())
            {
                ModelState.AddModelError(string.Empty, "The flight already has booked seats. Can not edit");
                return View();
            }


            var model = new FlightViewModel(
                    flight.Id,
                    flight.DepartureCity,
                    flight.ArrivalCity,
                    flight.Duration,
                    flight.Price,
                    flight.Capacity,
                    flight.Capacity <= 0
                );


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(EditFlightViewModel model)
        {
            var flight = await _context.Flights
                .Include(f => f.FlightPassengers)
                .FirstOrDefaultAsync(f => f.Id == model.Id);

            // Check if the flight exists
            if (flight == null)
            {
                // If the flight doesn't exist
                ModelState.AddModelError(string.Empty, "A flight with the provided id does not exist");
                return View();
            }

            if (flight.FlightPassengers != null && flight.FlightPassengers.Any())
            {
                ModelState.AddModelError(string.Empty, "The flight already has booked seats. Can not edit");
                return View();
            }

            flight.DepartureCity = model.DepartureCity;
            flight.ArrivalCity = model.ArrivalCity;
            flight.Duration = model.Duration;
            flight.Price = model.Price;
            flight.Capacity = model.Capacity;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Reset()
        {
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> BookSeat(int id)
        {
            var flight = await _context.Flights.FirstOrDefaultAsync(f => f.Id == id);

            // Check if the flight exists
            if (flight == null)
            {
                // If the flight doesn't exist
                TempData["ErrorMessage"] = "A flight with the provided id does not exist";
                return RedirectToAction(nameof(Index));
            }

            if (flight.Capacity <= 0)
            {
                TempData["ErrorMessage"] = "A flight is full booked.";
                return RedirectToAction(nameof(Index));
            }

            var passengers = await _context.Passengers.ToListAsync();
            var passengerViewModels = passengers.Select(p => new PassengerViewModel(
                p.Id,
                p.FirstName,
                p.FamilyName
            )).ToList();

            var model = new FlightViewModel(
                id, 
                flight.DepartureCity, 
                flight.ArrivalCity, 
                flight.Duration, 
                flight.Price, 
                flight.Capacity,
                flight.Capacity <= 0,
                passengerViewModels
                );

            return View(model);
        }

        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.FlightPassengers)
                .FirstOrDefaultAsync(f => f.Id == id);

            // Check if the flight exists
            if (flight == null)
            {
                // If the flight doesn't exist
                TempData["ErrorMessage"] = "A flight with the provided id does not exist";
                return RedirectToAction(nameof(Index));
            }

            if (flight.FlightPassengers != null && flight.FlightPassengers.Any())
            {
                TempData["ErrorMessage"] = "The flight already has booked seats. Cannot delete.";
                return RedirectToAction(nameof(Index));
            }

            await _flightService.DeleteFlightAsync(flight);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.FlightPassengers)
                    .ThenInclude(fp => fp.Passenger)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flight == null)
            {
                return NotFound();
            }

            var viewModel = new FlightViewModel
            (
                flight.Id,
                flight.DepartureCity,
                flight.ArrivalCity,
                flight.Duration,
                flight.Price,
                flight.Capacity,
                flight.Capacity <=0,
                flight.FlightPassengers.Select(fp => new PassengerViewModel(fp.Passenger.Id, fp.Passenger.FirstName, fp.Passenger.FamilyName)).ToList()
            );

            return View(viewModel);
        }

    }
}