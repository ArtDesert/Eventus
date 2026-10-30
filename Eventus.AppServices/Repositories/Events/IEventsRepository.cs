using Eventus.AppServices.Data.Domain.Events;
using Eventus.AppServices.Data.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;

namespace Eventus.AppServices.Repositories.Events;

public interface IEventsRepository
{
    PaginatedEventsResult GetEventsByFilter(GetEventsByFilterRequest request);

    Event? GetEventById(Guid id);

    Event CreateEvent(CreateEventRequest request);

    bool UpdateEvent(UpdateEventRequest request);

    bool DeleteEvent(Guid id);
}