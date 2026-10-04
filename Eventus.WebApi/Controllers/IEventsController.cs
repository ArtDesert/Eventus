using Eventus.WebApi.Data.Dto.Events;
using Eventus.WebApi.Data.Requests.Events;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.WebApi.Controllers;

public interface IEventsController
{
    ActionResult<List<EventDto>> GetAll();

    ActionResult<EventDto> GetEventById(Guid id);

    IActionResult CreateEvent(EventBaseRequest request);

    IActionResult UpdateEvent(Guid id, EventBaseRequest request);

    IActionResult DeleteEvent(Guid id);
}