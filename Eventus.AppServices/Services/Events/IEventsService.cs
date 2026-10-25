using Eventus.Api.Contracts.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;

namespace Eventus.AppServices.Services.Events;

public interface IEventsService
{
    PaginatedEventsResultDto GetEventsByFilter(GetEventsByFilterRequest request);

    EventDto? GetEventById(Guid id);

    Guid CreateEvent(CreateEventRequest request);

    void UpdateEvent(UpdateEventRequest request);

    void DeleteEvent(Guid id);
}