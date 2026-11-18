using System.Diagnostics.CodeAnalysis;
using Eventus.AppServices.Exceptions;

namespace Eventus.AppServices.Data.Domain.Events;

public class Event
{
    public Guid Id { get; }

    public string Title { get; }

    public string? Description { get; }

    public DateTime StartAt { get; }

    public DateTime EndAt { get; }

    public int TotalSeats { get; }

    public int AvailableSeats { get; }

    private Event(
        Guid id,
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        int totalSeats,
        int availableSeats)
    {
        if (id == Guid.Empty)
        {
            throw new DomainValidationException("Event id is default.", nameof(Id));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Title is required.", nameof(Title));
        }

        if (startAt == default)
        {
            throw new DomainValidationException("Start date must be filled.", nameof(StartAt));
        }

        if (endAt == default)
        {
            throw new DomainValidationException("End date must be filled.", nameof(EndAt));
        }

        if (startAt > endAt)
        {
            throw new DomainValidationException("Start date must not be later than end date.", nameof(StartAt));
        }

        if (totalSeats <= 0)
        {
            throw new DomainValidationException("Total count of seats must be more than zero.", nameof(TotalSeats));
        }

        if (availableSeats < 0 || availableSeats > totalSeats)
        {
            throw new DomainValidationException(
                "Available count of seats must not be less than zero or more than total count of seats.",
                nameof(AvailableSeats));
        }

        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = availableSeats;
    }

    public Event(Guid id, string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
        : this(id, title, description, startAt, endAt, totalSeats, totalSeats)
    {
    }

    public Event Update(string title, string? description, DateTime startAt, DateTime endAt)
    {
        return new Event(Id, title, description, startAt, endAt, TotalSeats, AvailableSeats);
    }

    public bool TryReserveSeats([NotNullWhen(true)] out Event? newEvent, int count = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        if (count > AvailableSeats)
        {
            newEvent = null;
            return false;
        }

        newEvent = new Event(Id, Title, Description, StartAt, EndAt, TotalSeats, AvailableSeats - count);
        return true;
    }

    public Event ReleaseSeats(int count = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        if (count > TotalSeats - AvailableSeats)
        {
            throw new InvalidOperationException(
                $"Cannot release {count} seats: only {TotalSeats - AvailableSeats} reserved for event {Id}.");
        }

        return new Event(Id, Title, Description, StartAt, EndAt, TotalSeats, AvailableSeats + count);
    }
}