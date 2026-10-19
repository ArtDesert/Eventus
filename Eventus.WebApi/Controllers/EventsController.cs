using Eventus.AppServices.Services.Events;
using Eventus.WebApi.Data.Dto.Events;
using Eventus.WebApi.Data.Requests.Events;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventsService eventsService)
    : ControllerBase, IEventsController
{
    [HttpGet]
    public ActionResult<List<EventDto>> GetAll() =>
        Ok(eventsService.GetAll());
    
    [HttpGet("{id:guid}")]
    public ActionResult<EventDto> GetEventById([FromRoute] Guid id)
    {
        var @event = eventsService.GetEventById(id);
        if (@event == null)
        {
            return NotFound($"Event with ID: {id} not found");
        }

        return Ok(@event);
    }
    
    [HttpPost]
    public IActionResult CreateEvent( [FromBody] EventBaseRequest request)
    {
        var appServicesRequest = new Eventus.AppServices.Data.Requests.Events.CreateEventRequest(
            request.Title, request.Description, request.StartAt, request.EndAt);
        var id = eventsService.CreateEvent(appServicesRequest);
        return CreatedAtAction(nameof(GetEventById), new { id }, id);
    }
    
    [HttpPut("{id:guid}")]
    public IActionResult UpdateEvent( [FromRoute] Guid id, [FromBody] EventBaseRequest request)
    {
        var appServicesRequest = new Eventus.AppServices.Data.Requests.Events.UpdateEventRequest(
            id, request.Title, request.Description, request.StartAt, request.EndAt);
        var isSuccess = eventsService.UpdateEvent(appServicesRequest);

        if (!isSuccess)
        {
            return NotFound($"Event with ID: {id} not found");
        }

        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    public IActionResult DeleteEvent([FromRoute] Guid id)
    {
        var isSuccess = eventsService.DeleteEvent(id);
        if (!isSuccess)
        {
            return NotFound($"Event with ID: {id} not found");
        }

        return NoContent();
    }
}