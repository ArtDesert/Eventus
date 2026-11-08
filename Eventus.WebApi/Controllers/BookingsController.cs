using Eventus.Api.Contracts.Dto.Bookings;
using Eventus.AppServices.Services.Bookings;
using Eventus.WebApi.Controllers.WebRoutes;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.WebApi.Controllers;

[ApiController]
public class BookingsController(IBookingsService bookingsService) : ControllerBase, IBookingsController
{
    [HttpPost(BookingsControllerWebRoutes.CreateBooking)]
    public async Task<ActionResult<BookingDto>> CreateBooking(
        [FromRoute] Guid eventId,
        CancellationToken token)
    {
        var createdBooking = await bookingsService.CreateBookingAsync(eventId, token);
        return AcceptedAtAction(nameof(GetBookingById), new { id = createdBooking.Id }, createdBooking);
    }

    [HttpGet(BookingsControllerWebRoutes.GetBookingById)]
    public async Task<ActionResult<BookingDto>> GetBookingById(
        [FromRoute] Guid id,
        CancellationToken token)
    {
        return await bookingsService.GetBookingByIdAsync(id, token);
    }
}