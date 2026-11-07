using Eventus.Api.Contracts.Dto.Bookings;
using Eventus.AppServices.Services.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.WebApi.Controllers;

[ApiController]
[Route("api")]
public class BookingsController(IBookingsService bookingsService) : ControllerBase, IBookingsController
{
    [HttpPost("events/{eventId:guid}/book")]
    public async Task<ActionResult<BookingDto>> CreateBooking(
        [FromRoute] Guid eventId,
        CancellationToken token)
    {
        var createdBooking = await bookingsService.CreateBookingAsync(eventId, token);
        return AcceptedAtAction(nameof(GetBookingById), new { id = createdBooking.Id }, createdBooking);
    }

    [HttpGet("bookings/{id:guid}")]
    public async Task<ActionResult<BookingDto>> GetBookingById(
        [FromRoute] Guid id,
        CancellationToken token)
    {
        return await bookingsService.GetBookingByIdAsync(id, token);
    }
}