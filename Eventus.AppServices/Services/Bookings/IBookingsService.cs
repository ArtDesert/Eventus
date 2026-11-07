using Eventus.Api.Contracts.Dto.Bookings;

namespace Eventus.AppServices.Services.Bookings;

public interface IBookingsService
{
    BookingDto CreateBookingAsync(Guid eventId);

    BookingDto GetBookingByIdAsync(Guid id);
    
    void CompleteCreationOfBooking(Guid bookingId);
}