using Eventus.AppServices.Data.Domain.Events;
using Eventus.AppServices.Data.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;

namespace Eventus.AppServices.Repositories.Events;

internal class EventsInMemoryRepository(IEnumerable<Event> seed) : IEventsRepository
{
    private readonly Lock _lock = new();
    private readonly List<Event> _events = [.. seed];

    public Task<PaginatedEventsResult> GetEventsByFilterAsync(
        GetEventsByFilterRequest request, CancellationToken token)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(request.Page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(request.PageSize, 1);

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

            return Task.FromResult(new PaginatedEventsResult(
                totalCount,
                pageEvents));
        }
    }

    public Task<Event?> GetEventByIdAsync(Guid id, CancellationToken token)
    {
        lock (_lock)
        {
            return Task.FromResult(_events.Find(e => e.Id == id));
        }
    }

    public Task<Event> CreateEventAsync(CreateEventRequest request, CancellationToken token)
    {
        lock (_lock)
        {
            var @event = new Event(Guid.NewGuid(), request.Title, request.Description, request.StartAt, request.EndAt);
            _events.Add(@event);
            return Task.FromResult(@event);
        }
    }

    public Task<bool> UpdateEventAsync(UpdateEventRequest request, CancellationToken token)
    {
        lock (_lock)
        {
            var index = _events.FindIndex(e => e.Id == request.Id);
            if (index < 0)
            {
                return Task.FromResult(false);
            }

            _events[index] = new Event(
                request.Id,
                request.Title,
                request.Description,
                request.StartAt,
                request.EndAt);

            return Task.FromResult(true);
        }
    }

    public Task<bool> DeleteEventAsync(Guid id, CancellationToken token)
    {
        lock (_lock)
        {
            var count = _events.RemoveAll(e => e.Id == id);
            return Task.FromResult(count != 0);
        }
    }
}