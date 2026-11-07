using System.Collections.Concurrent;
using Eventus.AppServices.Data.Domain.Bookings;

namespace Eventus.AppServices.Repositories.Bookings;

internal class BookingsInMemoryRepository : IBookingsRepository
{
    private readonly ConcurrentDictionary<Guid, Booking> _bookings = new();
    
    public BookingsInMemoryRepository(IEnumerable<Booking> seed)
    {
        foreach (var booking in seed)
        {
            _bookings.TryAdd(booking.Id, booking);
        }
    }
    
    public Booking CreateBookingAsync(Guid eventId)
    {
        var guid = Guid.NewGuid();
        var booking = new Booking(guid, eventId);
        _bookings.TryAdd(guid, booking);
        return booking;
    }

    public Booking? GetBookingByIdAsync(Guid id) => _bookings.GetValueOrDefault(id);
    
    public bool TryUpdateBooking(Booking current, Booking updated)
        => _bookings.TryUpdate(updated.Id, updated, current);
}