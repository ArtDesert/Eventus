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
    public BookingDto CreateBookingAsync(Guid eventId)
    {
        var @event = eventsService.GetEventById(eventId);
        if (@event == null)
        {
            throw new NotFoundException($"Event with ID: {eventId} not found.");
        }

        var createdBooking = repository.CreateBookingAsync(eventId);
        var task = new BookingTask(Guid.NewGuid(), createdBooking.Id);
        bookingTaskQueue.Enqueue(task);
        
        return MapToBookingDto(createdBooking);
    }

    public BookingDto GetBookingByIdAsync(Guid id)
    {
        var booking = repository.GetBookingByIdAsync(id);
        if (booking == null)
        {
            throw new NotFoundException($"Booking with ID: {id} not found.");
        }

        return MapToBookingDto(booking);
    }

    public void CompleteCreationOfBooking(Guid bookingId)
    {
        var booking = repository.GetBookingByIdAsync(bookingId);
        if (booking == null)
        {
            throw new NotFoundException($"Booking with ID: {bookingId} not found.");
        }

        // todo: check if an event was deleted

        var completedBooking = booking.CompleteCreation();
        if (!repository.TryUpdateBooking(booking, completedBooking))
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