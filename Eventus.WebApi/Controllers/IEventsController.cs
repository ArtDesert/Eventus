using Eventus.Api.Contracts.Dto.Events;
using Eventus.Api.Contracts.Requests.Events;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.WebApi.Controllers;

public interface IEventsController
{
    ActionResult<PaginatedEventsResultDto> GetEventsByFilter(GetEventsByFilterRequest request);

    ActionResult<EventDto> GetEventById(Guid id);

    IActionResult CreateEvent(EventBaseRequest request);

    IActionResult UpdateEvent(Guid id, EventBaseRequest request);

    IActionResult DeleteEvent(Guid id);
}