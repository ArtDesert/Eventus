using Eventus.Api.Contracts.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;

namespace Eventus.AppServices.Services.Events;

public interface IEventsService
{
    Task<PaginatedEventsResultDto> GetEventsByFilterAsync(GetEventsByFilterRequest request, CancellationToken token);

    Task<EventDto> GetEventByIdAsync(Guid id, CancellationToken token);

    Task<EventDto> CreateEventAsync(CreateEventRequest request, CancellationToken token);

    Task UpdateEventAsync(UpdateEventRequest request, CancellationToken token);

    Task DeleteEventAsync(Guid id, CancellationToken token);
}