namespace Eventus.AppServices.Data.Requests.Events;

public class CreateEventRequest(string title, string? description, DateTime startAt, DateTime endAt)
{
    public string Title { get; } = title;
    
    public string? Description { get; } =  description;
    
    public DateTime StartAt { get; } = startAt;
    
    public DateTime EndAt { get; } = endAt;
}