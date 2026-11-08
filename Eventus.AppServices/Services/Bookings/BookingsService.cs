using Eventus.Api.Contracts.Dto.Bookings;
using Eventus.Api.Contracts.Enums.Bookings;
using Eventus.AppServices.Data.Domain.Bookings;
using Eventus.AppServices.Exceptions;
using Eventus.AppServices.Queues.Bookings;
using Eventus.AppServices.Repositories.Bookings;
using Eventus.AppServices.Services.Events;

namespace Eventus.AppServices.Services.Bookings;

internal class BookingsService(
    IBookingsRepository repository,
    IEventsService eventsService,
    IBookingTaskQueue bookingTaskQueue) : IBookingsService
{
    public async Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken token)
    {
        var @event = await eventsService.GetEventByIdAsync(eventId, token);
        if (@event == null)
        {
            throw new NotFoundException($"Event with ID: {eventId} not found.");
        }

        var createdBooking = await repository.CreateBookingAsync(eventId, token);
        var task = new BookingTask(Guid.NewGuid(), createdBooking.Id);
        bookingTaskQueue.Enqueue(task);

        return MapToBookingDto(createdBooking);
    }

    public async Task<BookingDto> GetBookingByIdAsync(Guid id, CancellationToken token)
    {
        var booking = await repository.GetBookingByIdAsync(id, token);
        if (booking == null)
        {
            throw new NotFoundException($"Booking with ID: {id} not found.");
        }

        return MapToBookingDto(booking);
    }

    public async Task ConfirmBookingAsync(Guid bookingId, CancellationToken token)
    {
        var booking = await repository.GetBookingByIdAsync(bookingId, token);
        if (booking == null)
        {
            throw new NotFoundException($"Booking with ID: {bookingId} not found.");
        }

        // todo: check if an event was deleted

        var completedBooking = booking.Confirm();
        if (! await repository.TryUpdateBookingAsync(booking, completedBooking, token))
        {
            throw new BookingProcessingException("Booking was modified concurrently.");
        }
    }

    private static BookingDto MapToBookingDto(Booking booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = (BookingStatus)booking.Status,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt,
        };
    }
}