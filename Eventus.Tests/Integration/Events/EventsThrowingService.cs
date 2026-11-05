using Eventus.Api.Contracts.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;
using Eventus.AppServices.Services.Events;

namespace Eventus.Tests.Integration.Events;

public class EventsThrowingService : IEventsService
{
    public PaginatedEventsResultDto GetEventsByFilter(GetEventsByFilterRequest request)
    {
        throw new InvalidOperationException();
    }

    public EventDto GetEventById(Guid id)
    {
        throw new InvalidOperationException();
    }

    public EventDto CreateEvent(CreateEventRequest request)
    {
        throw new InvalidOperationException();
    }

    public void UpdateEvent(UpdateEventRequest request)
    {
        throw new InvalidOperationException();
    }

    public void DeleteEvent(Guid id)
    {
        throw new InvalidOperationException();
    }
}