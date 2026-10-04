namespace Eventus.AppServices.Data.Requests.Events;

public class UpdateEventRequest(Guid id, string title, string? description, DateTime startAt, DateTime endAt)
{
    public Guid Id { get; } = id;
    
    public string Title { get; } = title;
    
    public string? Description { get; } =  description;
    
    public DateTime StartAt { get; } = startAt;
    
    public DateTime EndAt { get; } = endAt;
}