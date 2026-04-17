using Airline_Ticket_System.Data.Entities;
using Airline_Ticket_System.Entities;
using Airline_Ticket_System.Infrastructure;
using Airline_Ticket_System.Models.Booking;
using Airline_Ticket_System.Repositories;
using Airline_Ticket_System.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace Airline_Ticket_System.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _db;

    public BookingService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<BookingCommitResult> TryCommitBookingAsync(
        BookSeatViewModel model,
        ClaimsPrincipal user,
        ApplicationUser? currentUser,
        CancellationToken cancellationToken = default)
    {
        var flight = await _db.Flights.FirstOrDefaultAsync(f => f.Id == model.FlightId, cancellationToken);
        if (flight == null)
        {
            await AttachPassengerDropdownAsync(model, null, cancellationToken);
            return BookingCommitResult.Fail("A flight with the provided id does not exist", model);
        }

        if (flight.Capacity <= 0)
        {
            await AttachPassengerDropdownAsync(model, flight, cancellationToken);
            return BookingCommitResult.Fail("The flight is fully booked.", model);
        }

        Passenger passenger;

        if (model.CreateNewPassenger)
        {
            if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.FamilyName))
            {
                await AttachPassengerDropdownAsync(model, flight, cancellationToken);
                return BookingCommitResult.Fail("Please provide both First and Family names for a new passenger.", model);
            }

            var existingPassenger = await _db.Passengers
                .FirstOrDefaultAsync(p =>
                    p.FirstName.ToLower() == model.FirstName.Trim().ToLower() &&
                    p.FamilyName.ToLower() == model.FamilyName.Trim().ToLower(), cancellationToken);

            if (existingPassenger != null)
            {
                passenger = existingPassenger;
            }
            else
            {
                passenger = new Passenger(model.FirstName.Trim(), model.FamilyName.Trim());
                _db.Passengers.Add(passenger);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
        else if (!string.IsNullOrEmpty(model.SelectedPassengerId))
        {
            // SelectedPassengerId now contains User ID, not Passenger ID
            var selectedUser = await _db.Users.FindAsync(model.SelectedPassengerId);
            if (selectedUser == null)
            {
                await AttachPassengerDropdownAsync(model, flight, cancellationToken);
                return BookingCommitResult.Fail("Selected user not found.", model);
            }

            // Find or create passenger record for this user
            var existingPassenger = await _db.Passengers
                .FirstOrDefaultAsync(p =>
                    p.FirstName.ToLower() == selectedUser.FirstName.Trim().ToLower() &&
                    p.FamilyName.ToLower() == selectedUser.FamilyName.Trim().ToLower(), cancellationToken);

            if (existingPassenger == null)
            {
                passenger = new Passenger(selectedUser.FirstName, selectedUser.FamilyName);
                _db.Passengers.Add(passenger);
                await _db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                passenger = existingPassenger;
            }
        }
        else if (model.IsBookingForSelf)
        {
            if (currentUser == null)
            {
                await AttachPassengerDropdownAsync(model, flight, cancellationToken);
                return BookingCommitResult.Fail("User context is required for self-booking.");
            }

            var existingPassenger = await _db.Passengers
                .FirstOrDefaultAsync(p =>
                    p.FirstName.ToLower() == currentUser.FirstName.Trim().ToLower() &&
                    p.FamilyName.ToLower() == currentUser.FamilyName.Trim().ToLower(), cancellationToken);

            if (existingPassenger == null)
            {
                passenger = new Passenger(currentUser.FirstName, currentUser.FamilyName);
                _db.Passengers.Add(passenger);
                await _db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                passenger = existingPassenger;
            }

            var isUserAlreadyBooked = await _db.FlightPassengers
                .AnyAsync(b => b.FlightId == flight.Id && b.PassengerId == passenger.Id && b.BookingStatus == "Confirmed", cancellationToken);

            if (isUserAlreadyBooked)
            {
                await AttachPassengerDropdownAsync(model, flight, cancellationToken);
                return BookingCommitResult.Fail("You have already booked this flight.");
            }
        }
        else
        {
            await AttachPassengerDropdownAsync(model, flight, cancellationToken);
            return BookingCommitResult.Fail("You must select or create a passenger.");
        }

        var alreadyBooked = await _db.FlightPassengers
            .AnyAsync(b => b.PassengerId == passenger.Id && b.FlightId == model.FlightId && b.BookingStatus == "Confirmed", cancellationToken);

        if (alreadyBooked)
        {
            await AttachPassengerDropdownAsync(model, flight, cancellationToken);
            return BookingCommitResult.Fail("This passenger has already booked this flight.");
        }

        string? committedPnr = null;
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var tracked = await _db.Flights
                .FirstOrDefaultAsync(f => f.Id == model.FlightId, cancellationToken);

            if (tracked == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                await AttachPassengerDropdownAsync(model, null, cancellationToken);
                return BookingCommitResult.Fail("A flight with the provided id does not exist", model);
            }

            if (tracked.Capacity <= 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                await AttachPassengerDropdownAsync(model, tracked, cancellationToken);
                return BookingCommitResult.Fail("The flight is fully booked.", model);
            }

            tracked.Capacity -= 1;

            var pnr = await GenerateUniquePnrAsync(cancellationToken);
            var booking = new FlightPassenger
            {
                FlightId = model.FlightId,
                PassengerId = passenger.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id,
                Pnr = pnr,
                BookingStatus = "Confirmed",
                PaymentAmount = tracked.Price,
                PaymentStatus = "Captured"
            };

            await _db.FlightPassengers.AddAsync(booking, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            committedPnr = pnr;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            await AttachPassengerDropdownAsync(model, flight, cancellationToken);
            return BookingCommitResult.Fail("The flight changed while booking (no seats left or concurrent update). Please refresh and try again.");
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            await transaction.RollbackAsync(cancellationToken);
            await AttachPassengerDropdownAsync(model, flight, cancellationToken);
            return BookingCommitResult.Fail("This passenger already has a booking on this flight.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return BookingCommitResult.Ok(committedPnr);
    }

    public async Task<CancelBookingResult> TryCancelBookingAsync(
        int flightPassengerId,
        ClaimsPrincipal user,
        ApplicationUser? currentUser,
        CancellationToken cancellationToken = default)
    {
        var fp = await _db.FlightPassengers
            .Include(x => x.Flight)
            .FirstOrDefaultAsync(x => x.Id == flightPassengerId, cancellationToken);

        if (fp == null)
            return CancelBookingResult.Fail("Booking not found.");

        if (fp.BookingStatus == "Cancelled")
            return CancelBookingResult.Fail("This booking is already cancelled.");

        var isAdmin = user.IsInRole("Admin");
        var isOperator = user.IsInRole("Operator");
        if (!isAdmin && !isOperator && fp.CreatedByUserId != currentUser?.Id)
            return CancelBookingResult.Fail("You are not allowed to cancel this booking.");

        var flight = fp.Flight;
        if (flight == null)
            return CancelBookingResult.Fail("Flight data is missing.");

        var hoursUntil = (flight.DepartureDateTime - DateTime.UtcNow).TotalHours;
        var refund = hoursUntil >= 24 ? fp.PaymentAmount ?? flight.Price : 0m;

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var trackedFlight = await _db.Flights.FirstOrDefaultAsync(f => f.Id == flight.Id, cancellationToken);
            if (trackedFlight == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return CancelBookingResult.Fail("Flight not found.");
            }

            fp.BookingStatus = "Cancelled";
            fp.CancelledAt = DateTime.UtcNow;
            fp.RefundAmount = refund;
            fp.PaymentStatus = refund > 0 ? "Refunded" : "Forfeited";
            trackedFlight.Capacity += 1;

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return CancelBookingResult.Fail("Could not cancel due to a concurrent update. Please try again.");
        }

        return CancelBookingResult.Ok(refund, fp.Pnr);
    }

    private async Task<string> GenerateUniquePnrAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 40; attempt++)
        {
            var code = PnrGenerator.NewCode();
            var taken = await _db.FlightPassengers.AnyAsync(f => f.Pnr == code, cancellationToken);
            if (!taken)
                return code;
        }

        throw new InvalidOperationException("Unable to allocate a unique PNR.");
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        for (var e = ex.InnerException; e != null; e = e.InnerException)
        {
            if (e.Message.Contains("IX_FlightPassengers_FlightId_PassengerId_Unique", StringComparison.OrdinalIgnoreCase)
                || e.Message.Contains("IX_FlightPassengers_Pnr_Unique", StringComparison.OrdinalIgnoreCase)
                || e.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
                || e.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private async Task AttachPassengerDropdownAsync(BookSeatViewModel model, Flight? flight, CancellationToken cancellationToken)
    {
        // Get all registered users regardless of IsActive status or role
        model.ExistingPassengers = await _db.Users
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.FamilyName} ({u.Email})"
            })
            .ToListAsync(cancellationToken);

        if (flight != null)
        {
            model.DepartureCity = flight.DepartureCity;
            model.ArrivalCity = flight.ArrivalCity;
            model.Duration = flight.Duration;
            model.Price = flight.Price;
        }
    }
}
