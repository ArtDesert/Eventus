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

    public Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken token)
    {
        var guid = Guid.NewGuid();
        var booking = new Booking(guid, eventId);
        _bookings.TryAdd(guid, booking);
        return Task.FromResult(booking);
    }

    public Task<Booking?> GetBookingByIdAsync(Guid id, CancellationToken token) =>
        Task.FromResult(_bookings.GetValueOrDefault(id));

    public Task<bool> TryUpdateBookingAsync(Booking current, Booking updated, CancellationToken token)
        => Task.FromResult(_bookings.TryUpdate(updated.Id, updated, current));
}