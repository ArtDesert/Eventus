using Eventus.AppServices.Exceptions;

namespace Eventus.AppServices.Data.Domain.Events;

public class Event
{
    public Guid Id { get; }

    public string Title { get; }

    public string? Description { get; }

    public DateTime StartAt { get; }

    public DateTime EndAt { get; }

    public Event(Guid id, string title, string? description, DateTime startAt, DateTime endAt)
    {
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

        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }
}