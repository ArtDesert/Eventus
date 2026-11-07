using Eventus.AppServices.Data.Domain.Bookings;

namespace Eventus.AppServices.Repositories.Bookings;

public interface IBookingsRepository
{
    Booking CreateBookingAsync(Guid eventId);
    
    Booking? GetBookingByIdAsync(Guid id);
    
    bool TryUpdateBooking(Booking current, Booking updated);
}