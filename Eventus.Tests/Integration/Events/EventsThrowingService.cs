using Eventus.Api.Contracts.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;
using Eventus.AppServices.Services.Events;

namespace Eventus.Tests.Integration.Events;

public class EventsThrowingService : IEventsService
{
    public Task<PaginatedEventsResultDto> GetEventsByFilterAsync(
        GetEventsByFilterRequest request, CancellationToken token)
    {
        throw new InvalidOperationException();
    }

    public Task<EventDto> GetEventByIdAsync(Guid id, CancellationToken token)
    {
        throw new InvalidOperationException();
    }

    public Task<EventDto> CreateEventAsync(CreateEventRequest request, CancellationToken token)
    {
        throw new InvalidOperationException();
    }

    public Task UpdateEventAsync(UpdateEventRequest request, CancellationToken token)
    {
        throw new InvalidOperationException();
    }

    public Task DeleteEventAsync(Guid id, CancellationToken token)
    {
        throw new InvalidOperationException();
    }
}