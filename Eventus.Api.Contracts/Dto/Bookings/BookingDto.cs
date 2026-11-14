using Eventus.Api.Contracts.Enums.Bookings;

namespace Eventus.Api.Contracts.Dto.Bookings;

public class BookingDto
{
    public required Guid Id { get; init; }

    public required Guid EventId { get; init; }

    public required BookingStatus Status { get; init; }

    public required DateTime CreatedAt { get; init; }

    public DateTime? ProcessedAt { get; init; }
}