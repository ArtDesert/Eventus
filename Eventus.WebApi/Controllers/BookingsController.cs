using Eventus.Api.Contracts.Dto.Bookings;
using Eventus.AppServices.Services.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.WebApi.Controllers;

[ApiController]
[Route("api")]
public class BookingsController(IBookingsService bookingsService) : ControllerBase, IBookingsController
{
    [HttpPost("events/{eventId:guid}/book")]
    public ActionResult<BookingDto> CreateBooking([FromRoute] Guid eventId)
    {
        var createdBooking = bookingsService.CreateBookingAsync(eventId);
        return AcceptedAtAction(nameof(GetBookingById), new { id = createdBooking.Id }, createdBooking);
    }

    [HttpGet("bookings/{id:guid}")]
    public ActionResult<BookingDto> GetBookingById([FromRoute] Guid id)
    {
        return bookingsService.GetBookingByIdAsync(id);
    }
}