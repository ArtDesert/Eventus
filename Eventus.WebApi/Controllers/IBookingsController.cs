using Eventus.Api.Contracts.Dto.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.WebApi.Controllers;

public interface IBookingsController
{
    ActionResult<BookingDto> CreateBooking(Guid eventId);

    ActionResult<BookingDto> GetBookingById(Guid id);
}