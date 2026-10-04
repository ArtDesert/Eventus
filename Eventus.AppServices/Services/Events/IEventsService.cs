using Eventus.AppServices.Data.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;

namespace Eventus.AppServices.Services.Events;

public interface IEventsService
{
    List<Event> GetAll();

    Event? GetEventById(Guid id);

    Guid CreateEvent(CreateEventRequest request);

    bool UpdateEvent(UpdateEventRequest request);

    bool DeleteEvent(Guid id);
}