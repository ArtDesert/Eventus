namespace Eventus.AppServices.Data.Requests.Events;

public class GetEventsByFilterRequest(string? title, DateTime? from, DateTime? to, int page, int pageSize)
{
    public string? Title { get; } = title;

    public DateTime? From { get; } = from;

    public DateTime? To { get; } = to;

    public int Page { get; } = page;

    public int PageSize { get; } = pageSize;
}