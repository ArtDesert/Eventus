using Eventus.AppServices.Data.Domain.Events;
using Eventus.AppServices.Data.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;

namespace Eventus.AppServices.Repositories.Events;

public interface IEventsRepository
{
    Task<PaginatedEventsResult> GetEventsByFilterAsync(
        GetEventsByFilterRequest request, CancellationToken token);

    Task<Event?> GetEventByIdAsync(Guid id, CancellationToken token);

    Task<Event> CreateEventAsync(CreateEventRequest request, CancellationToken token);

    Task<bool> UpdateEventAsync(UpdateEventRequest request, CancellationToken token);

    Task<bool> DeleteEventAsync(Guid id, CancellationToken token);
}