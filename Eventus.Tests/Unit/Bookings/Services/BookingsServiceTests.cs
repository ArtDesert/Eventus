using Eventus.Api.Contracts.Dto.Bookings;
using Eventus.AppServices.Data.Domain.Events;
using Eventus.AppServices.Data.Enums.Bookings;
using Eventus.AppServices.Exceptions;
using Eventus.AppServices.Queues.Bookings;
using Eventus.AppServices.Repositories.Bookings;
using Eventus.AppServices.Repositories.Events;
using Eventus.AppServices.Services.Bookings;

namespace Eventus.Tests.Unit.Bookings.Services;

public class BookingsServiceTests
{
    private readonly IEventsRepository _eventsRepository;
    private readonly IBookingsRepository _bookingsRepository;
    private readonly IBookingTaskQueue _queue;
    private readonly IBookingsService _service;
    private readonly CancellationToken _token = CancellationToken.None;

    public BookingsServiceTests()
    {
        _eventsRepository = new EventsInMemoryRepository([]);
        _bookingsRepository = new BookingsInMemoryRepository([]);
        _queue = new BookingTaskInMemoryQueue();
        _service = new BookingsService(_bookingsRepository, _eventsRepository, _queue);
    }

    private async Task<Event> CreateEventAsync(int totalSeats)
    {
        var @event = TestDataFactory.BuildEvent(eventId: Guid.NewGuid(), totalSeats: totalSeats);
        return await _eventsRepository.CreateEventAsync(@event, _token);
    }

    #region NegativeCases

    [Fact]
    public async Task CreateBooking_NonExistentEvent_ThrowsNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateBookingAsync(eventId, _token));
    }

    [Fact]
    public async Task CreateBooking_SeatsExhausted_ThrowsNoAvailableSeatsException()
    {
        // Arrange
        const int totalSeats = 1;
        var @event = await CreateEventAsync(totalSeats);
        await _service.CreateBookingAsync(@event.Id, _token);
        var eventWithoutSeats = await _eventsRepository.GetEventByIdAsync(@event.Id, _token);
        Assert.NotNull(eventWithoutSeats);
        Assert.Equal(0, eventWithoutSeats.AvailableSeats);

        // Act & Assert
        await Assert.ThrowsAsync<NoAvailableSeatsException>(() => _service.CreateBookingAsync(@event.Id, _token));
    }

    [Fact]
    public async Task RejectBooking_AlreadyRejectedBooking_DoesNotReleaseSeatTwice()
    {
        // Arrange
        const int totalSeats = 2;
        var @event = await CreateEventAsync(totalSeats);
        var booking = await _service.CreateBookingAsync(@event.Id, _token);
        await _service.CreateBookingAsync(@event.Id, _token);
        await _service.RejectBookingAsync(booking.Id, _token);
        var releasedEvent = await _eventsRepository.GetEventByIdAsync(@event.Id, _token);
        Assert.NotNull(releasedEvent);
        var oldAvailableSeats = releasedEvent.AvailableSeats;

        // Act
        await _service.RejectBookingAsync(booking.Id, _token);

        // Assert
        var updatedEvent = await _eventsRepository.GetEventByIdAsync(@event.Id, _token);
        Assert.NotNull(updatedEvent);
        Assert.Equal(oldAvailableSeats, updatedEvent.AvailableSeats);
    }

    #endregion

    #region PositiveCases

    [Fact]
    public async Task CreateBooking_EventWithAvailableSeats_BookingTaskWasEnqueued()
    {
        // Arrange
        const int totalSeats = 10;
        var @event = await CreateEventAsync(totalSeats);

        // Act
        var booking = await _service.CreateBookingAsync(@event.Id, _token);

        // Assert
        var result = _queue.TryDequeue(out var bookingTask);
        Assert.True(result);
        Assert.NotNull(bookingTask);
        Assert.Equal(booking.Id, bookingTask.BookingId);
    }

    [Fact]
    public async Task CreateBooking_EventWithAvailableSeats_DecreasesAvailableSeatsByOne()
    {
        // Arrange
        const int totalSeats = 10;
        var @event = await CreateEventAsync(totalSeats);

        // Act
        await _service.CreateBookingAsync(@event.Id, _token);

        // Assert
        var updatedEvent = await _eventsRepository.GetEventByIdAsync(@event.Id, _token);
        Assert.NotNull(updatedEvent);
        Assert.Equal(totalSeats - 1, updatedEvent.AvailableSeats);
    }

    [Fact]
    public async Task CreateBooking_UpToSeatsLimit_AllBookingsSucceedWithUniqueIds()
    {
        // Arrange
        const int totalSeats = 10;
        var @event = await CreateEventAsync(totalSeats);
        var bookings = new BookingDto[totalSeats];

        // Act
        for (int i = 0; i < bookings.Length; ++i)
        {
            bookings[i] = await _service.CreateBookingAsync(@event.Id, _token);
        }

        // Assert
        var updatedEvent = await _eventsRepository.GetEventByIdAsync(@event.Id, _token);
        Assert.NotNull(updatedEvent);
        Assert.Equal(0, updatedEvent.AvailableSeats);
        Assert.Distinct(bookings.Select(b => b.Id));
    }

    [Fact]
    public async Task RejectBooking_PendingBooking_RejectsBookingAndRestoresAvailableSeats()
    {
        // Arrange
        const int totalSeats = 10;
        var @event = await CreateEventAsync(totalSeats);
        var pendingBooking = await _service.CreateBookingAsync(@event.Id, _token);
        var reservedEvent = await _eventsRepository.GetEventByIdAsync(@event.Id, _token);
        Assert.NotNull(reservedEvent);
        Assert.Equal(totalSeats - 1, reservedEvent.AvailableSeats);

        // Act
        await _service.RejectBookingAsync(pendingBooking.Id, _token);

        // Assert
        var releasedEvent = await _eventsRepository.GetEventByIdAsync(@event.Id, _token);
        Assert.NotNull(releasedEvent);
        Assert.Equal(totalSeats, releasedEvent.AvailableSeats);

        var actual = await _service.GetBookingByIdAsync(pendingBooking.Id, _token);
        Assert.Equal(Api.Contracts.Enums.Bookings.BookingStatus.Rejected, actual.Status);
        Assert.NotNull(actual.ProcessedAt);
    }

    [Fact]
    public async Task RejectBooking_ThenCreateBooking_SameSeatCanBeBookedAgain()
    {
        // Arrange
        const int totalSeats = 1;
        var @event = await CreateEventAsync(totalSeats);
        var pendingBooking = await _service.CreateBookingAsync(@event.Id, _token);
        await _service.RejectBookingAsync(pendingBooking.Id, _token);
        var rejectedBooking = await _service.GetBookingByIdAsync(pendingBooking.Id, _token);
        Assert.Equal(Api.Contracts.Enums.Bookings.BookingStatus.Rejected, rejectedBooking.Status);
        Assert.NotNull(rejectedBooking.ProcessedAt);

        // Act
        var newBooking = await _service.CreateBookingAsync(@event.Id, _token);

        // Assert
        Assert.NotEqual(rejectedBooking.Id, newBooking.Id);
        Assert.Equal(Api.Contracts.Enums.Bookings.BookingStatus.Pending, newBooking.Status);

        var updatedEvent = await _eventsRepository.GetEventByIdAsync(@event.Id, _token);
        Assert.NotNull(updatedEvent);
        Assert.Equal(0, updatedEvent.AvailableSeats);
    }

    [Fact]
    public async Task ConfirmBooking_EventDeleted_RejectsBooking()
    {
        // Arrange
        const int totalSeats = 10;
        var @event = await CreateEventAsync(totalSeats);
        var booking = await _service.CreateBookingAsync(@event.Id, _token);
        var reservedEvent = await _eventsRepository.GetEventByIdAsync(@event.Id, _token);
        Assert.NotNull(reservedEvent);
        Assert.Equal(totalSeats - 1, reservedEvent.AvailableSeats);
        await _eventsRepository.DeleteEventAsync(@event.Id, _token);

        // Act
        var bookingResult = await _service.ConfirmBookingAsync(booking.Id, _token);

        // Assert
        Assert.Equal(BookingResult.Rejected, bookingResult);
        var updatedBooking = await _service.GetBookingByIdAsync(booking.Id, _token);
        Assert.Equal(Api.Contracts.Enums.Bookings.BookingStatus.Rejected, updatedBooking.Status);
        Assert.NotNull(updatedBooking.ProcessedAt);
    }

    [Fact]
    public async Task ConfirmBooking_RejectedBooking_ReturnsAlreadyProcessedResult()
    {
        // Arrange
        const int totalSeats = 10;
        var @event = await CreateEventAsync(totalSeats);
        var pendingBooking = await _service.CreateBookingAsync(@event.Id, _token);
        await _service.RejectBookingAsync(pendingBooking.Id, _token);
        var rejectedBooking = await _service.GetBookingByIdAsync(pendingBooking.Id, _token);

        // Act
        var bookingResult = await _service.ConfirmBookingAsync(rejectedBooking.Id, _token);

        // Assert
        Assert.Equal(BookingResult.AlreadyProcessed, bookingResult);
        var actualBooking = await _service.GetBookingByIdAsync(rejectedBooking.Id, _token);
        Assert.Equal(Api.Contracts.Enums.Bookings.BookingStatus.Rejected, actualBooking.Status);
        var actualEvent = await _eventsRepository.GetEventByIdAsync(@event.Id, _token);
        Assert.NotNull(actualEvent);
        Assert.Equal(totalSeats, actualEvent.AvailableSeats);
    }

    #endregion

    #region Concurrency

    /// <summary>
    /// 5 seats, 20 concurrent requests: exactly 5 succeed, 15 throw NoAvailableSeatsException, AvailableSeats == 0.
    /// </summary>
    [Fact]
    public async Task CreateBooking_ConcurrentOverBooking_OnlyAvailableSeatsAreBooked()
    {
        // Arrange
        const int totalSeats = 5;
        const int concurrentRequests = 20;
        var @event = await CreateEventAsync(totalSeats);

        // Act
        var exceptions = await Task.WhenAll(Enumerable.Range(0, concurrentRequests)
            .Select(_ => Record.ExceptionAsync(() => _service.CreateBookingAsync(@event.Id, _token))));

        // Assert
        var noExceptions = exceptions.Where(ex => ex == null).ToArray();
        Assert.Equal(totalSeats, noExceptions.Length);
        var noAvailableSeatsExceptions = exceptions.Where(ex => ex is NoAvailableSeatsException).ToArray();
        Assert.Equal(concurrentRequests - totalSeats, noAvailableSeatsExceptions.Length);

        var updatedEvent = await _eventsRepository.GetEventByIdAsync(@event.Id, _token);
        Assert.NotNull(updatedEvent);
        Assert.Equal(0, updatedEvent.AvailableSeats);
    }

    /// <summary>
    /// 10 seats, 10 concurrent requests: 10 bookings with unique ids.
    /// </summary>
    [Fact]
    public async Task CreateBooking_ConcurrentRequestsUpToSeatsLimit_AllBookingIdsAreUnique()
    {
        // Arrange
        const int totalSeats = 10;
        const int concurrentRequests = 10;
        var @event = await CreateEventAsync(totalSeats);

        // Act
        var bookings = await Task.WhenAll(Enumerable.Range(0, concurrentRequests)
            .Select(_ => _service.CreateBookingAsync(@event.Id, _token)));

        // Assert
        Assert.Distinct(bookings.Select(b => b.Id));

        var updatedEvent = await _eventsRepository.GetEventByIdAsync(@event.Id, _token);
        Assert.NotNull(updatedEvent);
        Assert.Equal(0, updatedEvent.AvailableSeats);
    }

    #endregion
}