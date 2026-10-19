using Eventus.AppServices.Data.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;

namespace Eventus.AppServices.Services.Events;

internal class EventsService : IEventsService
{
    private static readonly List<Event> Events =
    [
        new(
            Guid.Parse("5eb5fa1b-79d5-42b7-a190-a6532b5e085a"),
            "The International 2026",
            null,
            new DateTime(2026, 08, 13),
            new DateTime(2026, 08, 23))
    ];

    public List<Event> GetAll() => Events;

    public Event? GetEventById(Guid id) =>
        Events.Find(e => e.Id == id);

    public Guid CreateEvent(CreateEventRequest request)
    {
        var @event = new Event(Guid.NewGuid(), request.Title, request.Description, request.StartAt, request.EndAt);
        Events.Add(@event);
        return @event.Id;
    }

    public bool UpdateEvent(UpdateEventRequest request)
    {
        var @event = Events.Find(e => e.Id == request.Id);
        if (@event == null)
        {
            return false;
        }

        @event.Title = request.Title;
        @event.Description = request.Description;
        @event.StartAt = request.StartAt;
        @event.EndAt = request.EndAt;

        return true;
    }

    public bool DeleteEvent(Guid id)
    {
        var count = Events.RemoveAll(e => e.Id == id);
        return count != 0;
    }
}