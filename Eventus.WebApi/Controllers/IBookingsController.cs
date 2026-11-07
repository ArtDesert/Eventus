using Eventus.Api.Contracts.Dto.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.WebApi.Controllers;

public interface IBookingsController
{
    Task<ActionResult<BookingDto>> CreateBooking(Guid eventId, CancellationToken token = default);

    Task<ActionResult<BookingDto>> GetBookingById(Guid id, CancellationToken token = default);
}