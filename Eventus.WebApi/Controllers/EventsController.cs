using Eventus.Api.Contracts.Dto.Events;
using Eventus.Api.Contracts.Requests.Events;
using Eventus.AppServices.Services.Events;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventsService eventsService)
    : ControllerBase, IEventsController
{
    [HttpGet]
    public ActionResult<PaginatedEventsResultDto> GetEventsByFilter([FromQuery] GetEventsByFilterRequest request)
    {
        var appServicesRequest = new Eventus.AppServices.Data.Requests.Events.GetEventsByFilterRequest(
            request.Title,
            request.From,
            request.To,
            request.Page ?? 1,
            request.PageSize ?? 10);
        
        return eventsService.GetEventsByFilter(appServicesRequest);
    }
    
    [HttpGet("{id:guid}")]
    public ActionResult<EventDto> GetEventById([FromRoute] Guid id) =>
        eventsService.GetEventById(id)!;
    
    [HttpPost]
    public IActionResult CreateEvent([FromBody] EventBaseRequest request)
    {
        var appServicesRequest = new AppServices.Data.Requests.Events.CreateEventRequest(
            request.Title, request.Description, request.StartAt, request.EndAt);
        var id = eventsService.CreateEvent(appServicesRequest);
        return CreatedAtAction(nameof(GetEventById), new { id }, id);
    }
    
    [HttpPut("{id:guid}")]
    public IActionResult UpdateEvent([FromRoute] Guid id, [FromBody] EventBaseRequest request)
    {
        var appServicesRequest = new AppServices.Data.Requests.Events.UpdateEventRequest(
            id, request.Title, request.Description, request.StartAt, request.EndAt);
        eventsService.UpdateEvent(appServicesRequest);

        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    public IActionResult DeleteEvent([FromRoute] Guid id)
    {
        eventsService.DeleteEvent(id);

        return NoContent();
    }
}