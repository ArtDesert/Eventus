using System.Diagnostics;
using Eventus.Api.Contracts.Dto.Bookings;
using Eventus.AppServices.Data;
using Eventus.AppServices.Data.Domain.Bookings;
using Eventus.AppServices.Data.Enums.Bookings;
using Eventus.AppServices.Exceptions;
using Eventus.AppServices.Queues.Bookings;
using Eventus.AppServices.Repositories.Bookings;
using Eventus.AppServices.Repositories.Events;

namespace Eventus.AppServices.Services.Bookings;

internal class BookingsService(
    IBookingsRepository bookingsRepository,
    IEventsRepository eventsRepository,
    IBookingTaskQueue bookingTaskQueue) : IBookingsService
{
    public async Task<BookingDto> CreateBookingAsync(Guid eventId, CancellationToken token)
    {
        Booking createdBooking;
        await EventsBookingsSemaphore.Semaphore.WaitAsync(token);
        try
        {
            // todo Replace with a transaction after switching to the database.
            var @event = await eventsRepository.GetEventByIdAsync(eventId, token);
            if (@event == null)
            {
                throw new NotFoundException($"Event with ID: {eventId} not found.");
            }

            if (!@event.TryReserveSeats(out var updated))
            {
                throw new NoAvailableSeatsException($"No available seats for event {@event.Id}.");
            }

            if (!await eventsRepository.TryUpdateEventAsync(@event, updated, token))
            {
                throw new ConcurrentEventModificationException(
                    $"Event {@event.Id} was modified concurrently.");
            }

            // Point of no return: the booking is created even if the request is canceled.
            createdBooking = await bookingsRepository.CreateBookingAsync(eventId, CancellationToken.None);
            var task = new BookingTask(Guid.NewGuid(), createdBooking.Id);
            bookingTaskQueue.Enqueue(task);
        }
        finally
        {
            EventsBookingsSemaphore.Semaphore.Release();
        }

        return MapToBookingDto(createdBooking);
    }

    public async Task<BookingDto> GetBookingByIdAsync(Guid id, CancellationToken token)
    {
        var booking = await bookingsRepository.GetBookingByIdAsync(id, token);
        if (booking == null)
        {
            throw new NotFoundException($"Booking with ID: {id} not found.");
        }

        return MapToBookingDto(booking);
    }

    public async Task<BookingResult> ConfirmBookingAsync(Guid bookingId, CancellationToken token)
    {
        await EventsBookingsSemaphore.Semaphore.WaitAsync(token);
        try
        {
            var booking = await bookingsRepository.GetBookingByIdAsync(bookingId, token);
            if (booking == null)
            {
                throw new NotFoundException($"Booking with ID: {bookingId} not found.");
            }

            BookingResult result;

            switch (booking.Status)
            {
                case BookingStatus.Pending:
                    {
                        var @event = await eventsRepository.GetEventByIdAsync(booking.EventId, token);
                        Booking? updatedBooking;
                        if (@event == null)
                        {
                            updatedBooking = booking.Reject();
                            result = BookingResult.Rejected;
                        }
                        else
                        {
                            updatedBooking = booking.Confirm();
                            result = BookingResult.Confirmed;
                        }

                        if (!await bookingsRepository.TryUpdateBookingAsync(booking, updatedBooking, token))
                        {
                            throw new ConcurrentBookingModificationException(
                                $"Booking {booking.Id} was modified concurrently.");
                        }

                        break;
                    }
                case BookingStatus.Confirmed or BookingStatus.Rejected:
                    {
                        result = BookingResult.AlreadyProcessed;
                        break;
                    }
                default:
                    throw new UnreachableException(
                        $"Booking {booking.Id} has an incorrect status {booking.Status}.");
            }

            return result;
        }
        finally
        {
            EventsBookingsSemaphore.Semaphore.Release();
        }
    }

    public async Task<bool> RejectBookingAsync(Guid bookingId, CancellationToken token)
    {
        await EventsBookingsSemaphore.Semaphore.WaitAsync(token);
        try
        {
            var booking = await bookingsRepository.GetBookingByIdAsync(bookingId, token);

            if (booking == null || !booking.TryReject(out var rejected))
            {
                // Nothing to reject.
                return false;
            }

            if (!await bookingsRepository.TryUpdateBookingAsync(booking, rejected, token))
            {
                throw new ConcurrentBookingModificationException($"Booking {booking.Id} was modified concurrently.");
            }

            var @event = await eventsRepository.GetEventByIdAsync(booking.EventId, token);
            if (@event != null &&
                !await eventsRepository.TryUpdateEventAsync(@event, @event.ReleaseSeats(), token))
            {
                throw new ConcurrentEventModificationException(
                    $"Event {@event.Id} was modified concurrently.");
            }

            return true;
        }
        finally
        {
            EventsBookingsSemaphore.Semaphore.Release();
        }
    }

    private static BookingDto MapToBookingDto(Booking booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = MapBookingStatus(booking.Status),
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt,
        };
    }

    private static Api.Contracts.Enums.Bookings.BookingStatus MapBookingStatus(BookingStatus status) =>
        status switch
        {
            BookingStatus.Unknown => Api.Contracts.Enums.Bookings.BookingStatus.Unknown,
            BookingStatus.Pending => Api.Contracts.Enums.Bookings.BookingStatus.Pending,
            BookingStatus.Confirmed => Api.Contracts.Enums.Bookings.BookingStatus.Confirmed,
            BookingStatus.Rejected => Api.Contracts.Enums.Bookings.BookingStatus.Rejected,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}