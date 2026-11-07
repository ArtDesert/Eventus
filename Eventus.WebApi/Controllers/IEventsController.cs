using Eventus.Api.Contracts.Dto.Events;
using Eventus.Api.Contracts.Requests.Events;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.WebApi.Controllers;

public interface IEventsController
{
    Task<ActionResult<PaginatedEventsResultDto>> GetEventsByFilter(
        GetEventsByFilterRequest request, CancellationToken token = default);

    Task<ActionResult<EventDto>> GetEventById(Guid id, CancellationToken token = default);

    Task<IActionResult> CreateEvent(EventBaseRequest request, CancellationToken token = default);

    Task<IActionResult> UpdateEvent(Guid id, EventBaseRequest request, CancellationToken token = default);

    Task<IActionResult> DeleteEvent(Guid id, CancellationToken token = default);
}