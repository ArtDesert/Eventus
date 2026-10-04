namespace Eventus.AppServices.Data.Dto.Events;

public class Event(Guid id, string title, string? description, DateTime startAt, DateTime endAt)
{
    public Guid Id { get; set; } = id;

    public string Title { get; set; } = title;

    public string? Description { get; set; } = description;

    public DateTime StartAt { get; set; } = startAt;

    public DateTime EndAt { get; set; } = endAt;
}