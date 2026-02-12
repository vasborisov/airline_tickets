using Airline_Ticket_System.Data.Entities;
using Airline_Ticket_System.Entities;
using Airline_Ticket_System.Models.Booking;
using Airline_Ticket_System.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Airline_Ticket_System.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create(int id)
        {
            var flight = await _context.Flights.FirstOrDefaultAsync(f => f.Id == id);
            if (flight == null)
            {
                ModelState.AddModelError(string.Empty, "A flight with the provided id does not exist");
                return RedirectToAction("Index", "Flight");
            }

            var model = new BookSeatViewModel { 
                FlightId = flight.Id,
                DepartureCity = flight.DepartureCity,
                ArrivalCity = flight.ArrivalCity,
                Duration = flight.Duration,
                Price = flight.Price
            };

            var currentUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole("User") && currentUser != null)
            {
                model.FirstName = currentUser.FirstName;
                model.FamilyName = currentUser.FamilyName;
                model.IsBookingForSelf = true;
            }
            else {
                var passengers = await _context.Passengers.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.FirstName} {p.FamilyName}"
                }).ToListAsync();

                model.ExistingPassengers = passengers;
                model.IsBookingForSelf = false;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookSeatViewModel model)
        {
            // First, validate the model state
            if (!ModelState.IsValid)
            {
                model.ExistingPassengers = await _context.Passengers
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.FirstName} {p.FamilyName}"
                    })
                    .ToListAsync();
                return View(model);
            }

            var flight = await _context.Flights.FirstOrDefaultAsync(f => f.Id == model.FlightId);

            // Ensure the flight exists
            if (flight == null)
            {
                ModelState.AddModelError(string.Empty, "A flight with the provided id does not exist");
                return View(model);
            }

            // Ensure the flight has availability
            if (flight.Capacity <= 0)
            {
                ModelState.AddModelError(string.Empty, "The flight is fully booked.");
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            Passenger passenger;

            if (model.CreateNewPassenger)
            {
                // If creating a new passenger, validate FirstName and FamilyName
                if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.FamilyName))
                {
                    ModelState.AddModelError("", "Please provide both First and Family names for a new passenger.");
                    model.ExistingPassengers = await _context.Passengers
                        .Select(p => new SelectListItem
                        {
                            Value = p.Id.ToString(),
                            Text = $"{p.FirstName} {p.FamilyName}"
                        })
                        .ToListAsync();
                    model.DepartureCity = flight.DepartureCity;
                    model.ArrivalCity = flight.ArrivalCity;
                    model.Duration = flight.Duration;
                    model.Price = flight.Price;
                    return View(model);
                }

                var existingPassenger = await _context.Passengers
                    .FirstOrDefaultAsync(p =>
                        p.FirstName.ToLower() == model.FirstName.Trim().ToLower() &&
                        p.FamilyName.ToLower() == model.FamilyName.Trim().ToLower());

                if (existingPassenger != null)
                {
                    // Use the existing passenger if found
                    passenger = existingPassenger;
                }
                else
                {
                    // Otherwise, create a new passenger
                    passenger = new Passenger(model.FirstName.Trim(), model.FamilyName.Trim());
                    _context.Passengers.Add(passenger);
                    await _context.SaveChangesAsync();
                }
            }
            else if (model.SelectedPassengerId.HasValue)
            {
                // If selecting an existing passenger, ensure one is selected
                passenger = await _context.Passengers.FindAsync(model.SelectedPassengerId.Value);
                if (passenger == null)
                {
                    ModelState.AddModelError("", "Selected passenger not found.");
                    model.ExistingPassengers = await _context.Passengers
                        .Select(p => new SelectListItem
                        {
                            Value = p.Id.ToString(),
                            Text = $"{p.FirstName} {p.FamilyName}"
                        }).ToListAsync();
                    model.DepartureCity = flight.DepartureCity;
                    model.ArrivalCity = flight.ArrivalCity;
                    model.Duration = flight.Duration;
                    model.Price = flight.Price;
                    return View(model);
                }
            }
            else if (model.IsBookingForSelf)
            {
                var existingPassenger = await _context.Passengers
                    .FirstOrDefaultAsync(p =>
                        p.FirstName.ToLower() == currentUser.FirstName.Trim().ToLower() &&
                        p.FamilyName.ToLower() == currentUser.FamilyName.Trim().ToLower());

                if (existingPassenger == null)
                {
                    // If no passenger with the same name is found, create a new passenger entry
                    passenger = new Passenger(currentUser.FirstName, currentUser.FamilyName);
                    _context.Passengers.Add(passenger);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // If passenger already exists, assign the existing passenger
                    passenger = existingPassenger;
                }

                var isUserAlreadyBooked = await _context.FlightPassengers
                        .AnyAsync(b => b.FlightId == flight.Id && b.PassengerId == passenger.Id);

                if (isUserAlreadyBooked)
                {
                    // If the passenger has already booked the flight, show an error
                    ModelState.AddModelError("", "You have already booked this flight.");
                    model.ExistingPassengers = await _context.Passengers
                        .Select(p => new SelectListItem
                        {
                            Value = p.Id.ToString(),
                            Text = $"{p.FirstName} {p.FamilyName}"
                        }).ToListAsync();
                    model.DepartureCity = flight.DepartureCity;
                    model.ArrivalCity = flight.ArrivalCity;
                    model.Duration = flight.Duration;
                    model.Price = flight.Price;
                    return View(model);
                }
            }
            else
            {
                // If neither a new passenger nor an existing passenger is selected, show an error
                ModelState.AddModelError("", "You must select or create a passenger.");
                model.ExistingPassengers = await _context.Passengers
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.FirstName} {p.FamilyName}"
                    }).ToListAsync();
                model.DepartureCity = flight.DepartureCity;
                model.ArrivalCity = flight.ArrivalCity;
                model.Duration = flight.Duration;
                model.Price = flight.Price;
                return View(model);
            }

            // Check if the passenger already booked the flight
            bool alreadyBooked = await _context.FlightPassengers
                .AnyAsync(b => b.PassengerId == passenger.Id && b.FlightId == model.FlightId);

            if (alreadyBooked)
            {
                ModelState.AddModelError("", "This passenger has already booked this flight.");
                model.ExistingPassengers = await _context.Passengers
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.FirstName} {p.FamilyName}"
                    }).ToListAsync();
                model.DepartureCity = flight.DepartureCity;
                model.ArrivalCity = flight.ArrivalCity;
                model.Duration = flight.Duration;
                model.Price = flight.Price;
                return View(model);
            }

            // Proceed with booking the flight
            
            flight.Capacity -= 1;

            var booking = new FlightPassenger
            {
                FlightId = model.FlightId,
                PassengerId = passenger.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id
            };

            _context.FlightPassengers.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Flight");
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyBooked()
        {
            var userId = _userManager.GetUserId(User);

            var bookings = await _context.FlightPassengers
                .Include(b => b.Flight)
                .Include(b => b.Passenger)
                .Where(fp => fp.CreatedByUserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

    }

}
