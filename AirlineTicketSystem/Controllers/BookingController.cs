using Airline_Ticket_System.Data.Entities;
using Airline_Ticket_System.Entities;
using Airline_Ticket_System.Models.Booking;
using Airline_Ticket_System.Repositories;
using Airline_Ticket_System.Services.Interfaces;
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
        private readonly IBookingService _bookingService;
        private readonly IEmailService _emailService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IBookingService bookingService,
            IEmailService emailService,
            ILogger<BookingController> logger)
        {
            _context = context;
            _userManager = userManager;
            _bookingService = bookingService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        [ActionName("Create")]
        public async Task<IActionResult> CreateAsync(int id)
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
            
            // Always provide the list of all registered users for selection
            var allUsers = await _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.FamilyName} ({u.Email})"
            }).ToListAsync();
            model.ExistingPassengers = allUsers;
            
            // For User role, pre-populate their information for self-booking
            if (User.IsInRole("User") && currentUser != null)
            {
                model.FirstName = currentUser.FirstName;
                model.FamilyName = currentUser.FamilyName;
                model.IsBookingForSelf = true;
            }
            else 
            {
                model.IsBookingForSelf = false;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreateAsync(BookSeatViewModel model)
        {
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

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                ModelState.AddModelError(string.Empty, "User not found");
                return View(model);
            }
            
            var outcome = await _bookingService.TryCommitBookingAsync(model, User, currentUser);

            if (outcome.Success)
            {
                if (!string.IsNullOrEmpty(outcome.Pnr))
                {
                    var fp = await _context.FlightPassengers
                        .AsNoTracking()
                        .Include(x => x.Passenger)
                        .Include(x => x.Flight)
                        .FirstOrDefaultAsync(x => x.Pnr == outcome.Pnr);
                        
                    _logger.LogInformation("Loaded FlightPassenger for PNR {PNR}: fp={FpExists}, passenger={PassengerExists}, flight={FlightExists}", 
                        outcome.Pnr, fp != null, fp?.Passenger != null, fp?.Flight != null);
                        
                    if (fp?.Passenger != null && fp.Flight != null)
                    {
                        // Send email to passenger if they have an email address
                        string? emailToSend = null;
                        
                        if (!string.IsNullOrEmpty(fp.Passenger.Email))
                        {
                            emailToSend = fp.Passenger.Email;
                        }
                        else if (!string.IsNullOrEmpty(currentUser?.Email))
                        {
                            // Fallback to booking user's email if passenger email not available
                            emailToSend = currentUser.Email;
                        }
                        
                        if (!string.IsNullOrEmpty(emailToSend))
                        {
                            try
                            {
                                await _emailService.SendBookingConfirmationAsync(
                                    emailToSend, outcome.Pnr, fp.Flight, fp.Passenger);
                            }
                            catch (Exception ex)
                            {
                                // Log the email failure but don't let it break the booking process
                                // The booking was successful, so we continue with the success flow
                                _logger.LogError(ex, "Failed to send booking confirmation email to {Email} for PNR {PNR}", 
                                    emailToSend, outcome.Pnr);
                                
                                // Add a user-friendly message that booking succeeded but email failed
                                TempData["EmailWarning"] = "Booking confirmed successfully, but we couldn't send the confirmation email. Please save your PNR number.";
                            }
                        }
                    }
                }

                TempData["BookingMessage"] = string.IsNullOrEmpty(outcome.Pnr)
                    ? "Booking completed."
                    : $"Booking confirmed. PNR: {outcome.Pnr}";

                // Redirect based on user role
                if (User.IsInRole("Admin") || User.IsInRole("Operator"))
                {
                    // Admin and Operator redirect back to flights list to continue working
                    return RedirectToAction("Index", "Flight");
                }
                else
                {
                    // Regular users go to their bookings page
                    return RedirectToAction("MyBooked", "Booking");
                }
            }

            if (!string.IsNullOrEmpty(outcome.ErrorKey))
            {
                ModelState.AddModelError(outcome.ErrorKey, outcome.ErrorMessage ?? string.Empty);
            }
            else
            {
                ModelState.AddModelError(string.Empty, outcome.ErrorMessage ?? "Unable to complete booking.");
            }

            return View(outcome.ViewModel ?? model);
        }

        [ActionName("MyBooked")]
        public async Task<IActionResult> MyBookedAsync()
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Cancel")]
        public async Task<IActionResult> CancelAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _bookingService.TryCancelBookingAsync(id, User, user);
            if (result.Success && user?.Email != null && !string.IsNullOrEmpty(result.Pnr))
            {
                try
                {
                    // Load flight details for the cancellation email
                    Flight? flight = null;
                    var flightPassenger = await _context.FlightPassengers
                        .Include(fp => fp.Flight)
                        .FirstOrDefaultAsync(fp => fp.Pnr == result.Pnr);
                    
                    if (flightPassenger?.Flight != null)
                    {
                        flight = flightPassenger.Flight;
                    }
                    
                    await _emailService.SendBookingCancelledAsync(user.Email, result.Pnr, result.RefundAmount, flight);
                }
                catch (Exception ex)
                {
                    // Log the email failure but don't let it break the cancellation process
                    _logger.LogError(ex, "Failed to send booking cancellation email to {Email} for PNR {PNR}", 
                        user.Email, result.Pnr);
                    
                    // Add a user-friendly message that cancellation succeeded but email failed
                    TempData["EmailWarning"] = "Booking cancelled successfully, but we couldn't send the cancellation email.";
                }
            }

            TempData["BookingMessage"] = result.Success
                ? $"Booking cancelled. Refund: {(result.RefundAmount ?? 0m):C}"
                : result.Message;
            return RedirectToAction("MyBooked");
        }

        [HttpGet]
        [ActionName("ByPnr")]
        public async Task<IActionResult> ByPnrAsync(string pnr)
        {
            if (string.IsNullOrWhiteSpace(pnr))
                return BadRequest();

            var key = pnr.Trim().ToUpperInvariant();
            var userId = _userManager.GetUserId(User);
            var fp = await _context.FlightPassengers
                .Include(x => x.Flight)
                .Include(x => x.Passenger)
                .FirstOrDefaultAsync(x => x.Pnr == key);

            if (fp == null)
                return NotFound();

            if (!User.IsInRole("Admin") && !User.IsInRole("Operator") && fp.CreatedByUserId != userId)
                return Forbid();

            return View("BookingDetails", fp);
        }

    }

}
