using Eventus.Api.Contracts.Dto.Bookings;
using Eventus.AppServices.Data.Enums.Bookings;

namespace Eventus.AppServices.Services.Bookings;

public interface IBookingsService
{
    Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken token);

    Task<BookingDto> GetBookingByIdAsync(Guid id, CancellationToken token);

    Task<BookingResult> ConfirmBookingAsync(Guid bookingId, CancellationToken token);

    Task<bool> RejectBookingAsync(Guid bookingId, CancellationToken token);
}