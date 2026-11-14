using Eventus.AppServices.Data.Domain.Bookings;

namespace Eventus.AppServices.Repositories.Bookings;

public interface IBookingsRepository
{
    Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken token);

    Task<Booking?> GetBookingByIdAsync(Guid id, CancellationToken token);

    Task<bool> TryUpdateBookingAsync(Booking current, Booking updated, CancellationToken token);
}