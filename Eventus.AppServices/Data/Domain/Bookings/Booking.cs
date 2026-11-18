using System.Diagnostics.CodeAnalysis;
using Eventus.AppServices.Data.Enums.Bookings;
using Eventus.AppServices.Exceptions;

namespace Eventus.AppServices.Data.Domain.Bookings;

public class Booking
{
    public Guid Id { get; }

    public Guid EventId { get; }

    public BookingStatus Status { get; }

    public DateTime CreatedAt { get; }

    public DateTime? ProcessedAt { get; }

    private Booking(Guid id, Guid eventId, BookingStatus status, DateTime createdAt, DateTime? processedAt)
    {
        if (!Enum.IsDefined(status) || status == BookingStatus.Unknown)
        {
            throw new DomainValidationException("Booking has an incorrect status.", nameof(Status));
        }

        if (id == Guid.Empty)
        {
            throw new DomainValidationException("Booking id is default.", nameof(Id));
        }

        if (eventId == Guid.Empty)
        {
            throw new DomainValidationException("Event id is default.", nameof(EventId));
        }

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

    public Booking Confirm()
    {
        if (Status == BookingStatus.Pending)
        {
            return new Booking(Id, EventId, BookingStatus.Confirmed, CreatedAt, DateTime.UtcNow);
        }

        throw new BookingProcessingException(
            $"You cannot complete the booking if the status is not pending. Current status is: \'{Status}\'");
    }
    
    public Booking Reject()
    {
        if (Status == BookingStatus.Pending)
        {
            return new Booking(Id, EventId, BookingStatus.Rejected, CreatedAt, DateTime.UtcNow);
        }

        throw new BookingProcessingException(
            $"You cannot reject the booking if the status is not pending. Current status is: \'{Status}\'");
    }

    public bool TryReject([NotNullWhen(true)] out Booking? rejected)
    {
        if (Status != BookingStatus.Pending)
        {
            rejected = null;
            return false;
        }

        rejected = new Booking(Id, EventId, BookingStatus.Rejected, CreatedAt, DateTime.UtcNow);
        return true;
    }
}