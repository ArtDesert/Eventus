using Eventus.Api.Contracts.Dto.Bookings;
using Eventus.AppServices.Services.Bookings;

namespace Eventus.Tests.Integration.Bookings;

public class BookingsThrowingService : IBookingsService
{
    public Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<BookingDto> GetBookingByIdAsync(Guid id, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task ConfirmBookingAsync(Guid bookingId, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}