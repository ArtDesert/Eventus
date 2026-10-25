namespace Eventus.AppServices.Data.Dto.Events;

public class Event(Guid id, string title, string? description, DateTime startAt, DateTime endAt)
{
    public Guid Id { get; init; } = id;

    public string Title { get; init; } = title;

    public string? Description { get; init; } = description;

    public DateTime StartAt { get; init; } = startAt;

    public DateTime EndAt { get; init; } = endAt;
}