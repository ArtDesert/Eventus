namespace Eventus.AppServices.Data.Dto.Events;

public class PaginatedEventsResult(int totalCount, Event[] items)
{
    public int TotalCount { get; init; } = totalCount;

    public Event[] Items { get; init; } = items;
}