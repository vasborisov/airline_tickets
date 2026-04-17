using Airline_Ticket_System.Data.Entities;
using Airline_Ticket_System.Entities;
using Airline_Ticket_System.Models.Flight;
using Airline_Ticket_System.Models.Passenger;
using Airline_Ticket_System.Repositories;
using Airline_Ticket_System.Repositories.Models;
using Airline_Ticket_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Airline_Ticket_System.Controllers
{
    public class FlightController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFlightService _flightService;
        private readonly IEmailService _emailService;
        private readonly ILogger<FlightController> _logger;

        public FlightController(
            ApplicationDbContext context, 
            IFlightService flightService,
            IEmailService emailService,
            ILogger<FlightController> logger)
        {
            _context = context;
            _flightService = flightService;
            _emailService = emailService;
            _logger = logger;
        }

        [ActionName("Index")]
        public async Task<IActionResult> IndexAsync(FlightSearchViewModel? search)
        {
            search ??= new FlightSearchViewModel();
            var criteria = new FlightSearchCriteria
            {
                DepartureCity = search.DepartureCity,
                ArrivalCity = search.ArrivalCity,
                FromDate = search.FromDate,
                ToDate = search.ToDate,
                MinPrice = search.MinPrice,
                MaxPrice = search.MaxPrice,
                Status = search.Status,
                SortBy = search.SortBy ?? "departure",
                Page = search.Page < 1 ? 1 : search.Page,
                PageSize = search.PageSize < 1 ? 15 : search.PageSize
            };

            var (items, total) = await _flightService.SearchFlightsAsync(criteria);
            var flights = (items ?? new List<Flight>()).Select(MapToViewModel).ToList();

            var page = new FlightIndexPageViewModel
            {
                Search = search,
                Flights = flights,
                TotalCount = total
            };
            return View(page);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ActionName("Create")]
        public IActionResult Create()
        {
            return View(new CreateFlightViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [ActionName("Create")]
        public async Task<IActionResult> CreateAsync(CreateFlightViewModel flightViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(flightViewModel);
            }

            var dep = flightViewModel.DepartureDateTime;
            var entity = new Flight
            {
                DepartureCity = flightViewModel.DepartureCity.Trim(),
                ArrivalCity = flightViewModel.ArrivalCity.Trim(),
                Duration = flightViewModel.Duration,
                Price = flightViewModel.Price,
                Capacity = flightViewModel.Capacity,
                DepartureDateTime = dep,
                ArrivalDateTime = dep.AddMinutes(flightViewModel.Duration),
                FlightNumber = flightViewModel.FlightNumber.Trim().ToUpperInvariant(),
                Status = "Scheduled",
                Gate = string.IsNullOrWhiteSpace(flightViewModel.Gate) ? null : flightViewModel.Gate.Trim()
            };

            await _flightService.AddFlightAsync(entity);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ActionName("Edit")]
        public async Task<IActionResult> EditAsync(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.FlightPassengers)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flight == null)
            {
                ModelState.AddModelError(string.Empty, "A flight with the provided id does not exist");
                return View(new EditFlightViewModel());
            }

            if (flight.FlightPassengers != null && flight.FlightPassengers.Any(fp => fp.BookingStatus == "Confirmed"))
            {
                ModelState.AddModelError(string.Empty, "The flight already has booked seats. Can not edit");
                return View(new EditFlightViewModel());
            }

            var model = new EditFlightViewModel
            {
                Id = flight.Id,
                DepartureCity = flight.DepartureCity,
                ArrivalCity = flight.ArrivalCity,
                Duration = flight.Duration,
                Price = flight.Price,
                Capacity = flight.Capacity,
                FlightNumber = flight.FlightNumber,
                DepartureDateTime = flight.DepartureDateTime,
                ArrivalDateTime = flight.ArrivalDateTime,
                Status = flight.Status,
                Gate = flight.Gate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [ActionName("Edit")]
        public async Task<IActionResult> EditAsync(EditFlightViewModel model)
        {
            // Check basic model validation first
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var flight = await _context.Flights
                .Include(f => f.FlightPassengers)
                    .ThenInclude(fp => fp.Passenger)
                .FirstOrDefaultAsync(f => f.Id == model.Id);

            if (flight == null)
            {
                ModelState.AddModelError(string.Empty, "A flight with the provided id does not exist");
                return View(model);
            }

            if (flight.FlightPassengers != null && flight.FlightPassengers.Any(fp => fp.BookingStatus == "Confirmed"))
            {
                // Allow operational changes (schedule, status, gate) but prevent structural changes (capacity, price)
                if (flight.Capacity != model.Capacity)
                {
                    ModelState.AddModelError("Capacity", "Cannot change capacity when flight has confirmed bookings");
                }
                if (flight.Price != model.Price)
                {
                    ModelState.AddModelError("Price", "Cannot change price when flight has confirmed bookings");
                }
                if (flight.DepartureCity != model.DepartureCity || flight.ArrivalCity != model.ArrivalCity)
                {
                    ModelState.AddModelError(string.Empty, "Cannot change departure or arrival cities when flight has confirmed bookings");
                }
                
                // If there are validation errors, return the view
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
            }

            // Check for significant changes that require passenger notification
            var hasScheduleChange = flight.DepartureDateTime != model.DepartureDateTime ||
                                  flight.ArrivalDateTime != model.ArrivalDateTime ||
                                  flight.Status != model.Status ||
                                  flight.Gate != model.Gate?.Trim();

            // Store passengers to notify before updating flight
            var passengersToNotify = new List<(string Email, Passenger Passenger)>();
            if (hasScheduleChange && flight.FlightPassengers != null)
            {
                passengersToNotify = flight.FlightPassengers
                    .Where(fp => fp.BookingStatus == "Confirmed" && fp.Passenger != null && fp.CreatedByUser != null && !string.IsNullOrEmpty(fp.CreatedByUser.Email))
                    .Select(fp => (fp.CreatedByUser!.Email!, fp.Passenger!))
                    .ToList();
            }

            // Update flight details
            flight.DepartureCity = model.DepartureCity;
            flight.ArrivalCity = model.ArrivalCity;
            flight.Duration = model.Duration;
            flight.Price = model.Price;
            flight.Capacity = model.Capacity;
            flight.FlightNumber = model.FlightNumber.Trim().ToUpperInvariant();
            flight.DepartureDateTime = model.DepartureDateTime;
            flight.ArrivalDateTime = model.ArrivalDateTime;
            flight.Status = model.Status;
            flight.Gate = string.IsNullOrWhiteSpace(model.Gate) ? null : model.Gate.Trim();

            await _context.SaveChangesAsync();

            // Send notifications to affected passengers
            if (hasScheduleChange && passengersToNotify.Any())
            {
                _logger.LogInformation("Sending flight schedule change notifications to {Count} passengers for flight {FlightNumber}", 
                    passengersToNotify.Count, flight.FlightNumber);

                foreach (var (email, passenger) in passengersToNotify)
                {
                    try
                    {
                        await _emailService.SendFlightScheduleChangedAsync(email, flight);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send flight schedule change notification to {Email} for flight {FlightNumber}", 
                            email, flight.FlightNumber);
                    }
                }

                TempData["SuccessMessage"] = $"Flight updated successfully. Notifications sent to {passengersToNotify.Count} passengers.";
            }
            else
            {
                TempData["SuccessMessage"] = "Flight updated successfully.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ActionName("Reset")]
        public IActionResult Reset()
        {
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        [ActionName("BookSeat")]
        public IActionResult BookSeat(int id)
        {
            return RedirectToAction("Create", "Booking", new { id });
        }

        [Authorize(Roles = "Admin")]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.FlightPassengers)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flight == null)
            {
                TempData["ErrorMessage"] = "A flight with the provided id does not exist";
                return RedirectToAction("Index");
            }

            if (flight.FlightPassengers != null && flight.FlightPassengers.Any(fp => fp.BookingStatus == "Confirmed"))
            {
                TempData["ErrorMessage"] = "The flight already has booked seats. Cannot delete.";
                return RedirectToAction("Index");
            }

            await _flightService.DeleteFlightAsync(flight);

            return RedirectToAction("Index");
        }

        [ActionName("Details")]
        public async Task<IActionResult> DetailsAsync(int id)
        {
            var flight = await _context.Flights
                .Include(f => f.FlightPassengers)
                    .ThenInclude(fp => fp.Passenger)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flight == null)
            {
                return NotFound();
            }

            return View(MapToViewModel(flight));
        }

        private static FlightViewModel MapToViewModel(Flight f)
        {
            var passengers = f.FlightPassengers
                .Where(fp => fp.BookingStatus == "Confirmed" && fp.Passenger != null)
                .Select(fp => new PassengerViewModel(fp.Passenger!.Id, fp.Passenger.FirstName, fp.Passenger.FamilyName))
                .ToList();

            var vm = new FlightViewModel(
                f.Id,
                f.DepartureCity,
                f.ArrivalCity,
                f.Duration,
                f.Price,
                f.Capacity,
                f.Capacity <= 0,
                passengers)
            {
                DepartureDateTime = f.DepartureDateTime,
                FlightNumber = f.FlightNumber,
                Status = f.Status,
                Gate = f.Gate
            };
            return vm;
        }
    }
}
