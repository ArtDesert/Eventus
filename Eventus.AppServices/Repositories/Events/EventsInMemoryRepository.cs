using Eventus.AppServices.Data.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;

namespace Eventus.AppServices.Repositories.Events;

internal class EventsInMemoryRepository(IEnumerable<Event> seed) : IEventsRepository
{
    private readonly Lock _lock = new();
    private readonly List<Event> _events = [.. seed];

    public PaginatedEventsResult GetEventsByFilter(GetEventsByFilterRequest request)
    {
        lock (_lock)
        {
            var filteredEvents = _events
                .Where(e => (string.IsNullOrWhiteSpace(request.Title) ||
                             e.Title.Contains(request.Title, StringComparison.InvariantCultureIgnoreCase))
                            && (request.From == null || e.StartAt >= request.From)
                            && (request.To == null || e.EndAt <= request.To))
                .ToArray();

            var totalCount = filteredEvents.Length;
            var pageEvents = filteredEvents
                .OrderBy(e => e.StartAt)
                .ThenBy(e => e.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToArray();

            return new PaginatedEventsResult(
                totalCount,
                pageEvents);
        }
    }

    public Event? GetEventById(Guid id)
    {
        lock (_lock)
        {
            return _events.Find(e => e.Id == id);
        }
    }

    public Guid CreateEvent(CreateEventRequest request)
    {
        lock (_lock)
        {
            var @event = new Event(Guid.NewGuid(), request.Title, request.Description, request.StartAt, request.EndAt);
            _events.Add(@event);
            return @event.Id;
        }
    }

    public bool UpdateEvent(UpdateEventRequest request)
    {
        lock (_lock)
        {
            var index = _events.FindIndex(e => e.Id == request.Id);
            if (index < 0)
            {
                return false;
            }
            
            _events[index] = new Event(
                request.Id,
                request.Title,
                request.Description,
                request.StartAt,
                request.EndAt);

            return true;
        }
    }

    public bool DeleteEvent(Guid id)
    {
        lock (_lock)
        {
            var count = _events.RemoveAll(e => e.Id == id);
            return count != 0;
        }
    }
}