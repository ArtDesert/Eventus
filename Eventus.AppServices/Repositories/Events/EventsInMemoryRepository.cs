using System.Collections.Concurrent;
using Eventus.AppServices.Data.Domain.Events;
using Eventus.AppServices.Data.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;

namespace Eventus.AppServices.Repositories.Events;

internal class EventsInMemoryRepository : IEventsRepository
{
    private readonly ConcurrentDictionary<Guid, Event> _events = new();

    public EventsInMemoryRepository(IEnumerable<Event> seed)
    {
        foreach (var @event in seed)
        {
            _events.TryAdd(@event.Id, @event);
        }
    }

    public Task<PaginatedEventsResult> GetEventsByFilterAsync(
        GetEventsByFilterRequest request, CancellationToken token)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(request.Page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(request.PageSize, 1);

        var filteredEvents = _events.Values
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

    public Task<Event?> GetEventByIdAsync(Guid id, CancellationToken token) =>
        Task.FromResult(_events.GetValueOrDefault(id));

    public Task<Event> CreateEventAsync(Event @event, CancellationToken token)
    {
        if (!_events.TryAdd(@event.Id, @event))
        {
            throw new InvalidOperationException($"Event with ID {@event.Id} already exists.");
        }

        return Task.FromResult(@event);
    }

    public Task<bool> TryUpdateEventAsync(Event current, Event updated, CancellationToken token) =>
        Task.FromResult(_events.TryUpdate(updated.Id, updated, current));

    public Task<bool> DeleteEventAsync(Guid id, CancellationToken token) =>
        Task.FromResult(_events.TryRemove(id, out _));
}