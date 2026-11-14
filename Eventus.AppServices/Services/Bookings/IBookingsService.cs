using Eventus.Api.Contracts.Dto.Bookings;

namespace Eventus.AppServices.Services.Bookings;

public interface IBookingsService
{
    Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken token);

    Task<BookingDto> GetBookingByIdAsync(Guid id, CancellationToken token);

    Task ConfirmBookingAsync(Guid bookingId, CancellationToken token);
}