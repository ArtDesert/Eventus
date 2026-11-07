using Eventus.AppServices.Data.Enums.Bookings;
using Eventus.AppServices.Exceptions;

namespace Eventus.AppServices.Data.Domain.Bookings;

public class Booking
{
    public Guid Id { get; }

    public Guid EventId { get; }

    public BookingStatus Status { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime? ProcessedAt { get; private set; }

    private Booking(Guid id, Guid eventId, BookingStatus status, DateTime createdAt, DateTime? processedAt)
    {
        Id = id;
        EventId = eventId;
        Status = status;
        CreatedAt = createdAt;
        ProcessedAt = processedAt;
    }

    public Booking(Guid id, Guid eventId) :
        this(id, eventId, BookingStatus.Pending, DateTime.UtcNow, null)
    {
    }

    public Booking CompleteCreation()
    {
        if (Status == BookingStatus.Pending)
        {
            return new Booking(Id, EventId, BookingStatus.Confirmed, CreatedAt, DateTime.UtcNow);
        }

        throw new BookingProcessingException(
            $"You cannot complete the creation of booking if the status is not pending. Current status is: \'{Status}\'");
    }
}