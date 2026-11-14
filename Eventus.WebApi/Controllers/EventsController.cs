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
    public async Task<ActionResult<PaginatedEventsResultDto>> GetEventsByFilter(
        [FromQuery] GetEventsByFilterRequest request, CancellationToken token)
    {
        var appServicesRequest = new Eventus.AppServices.Data.Requests.Events.GetEventsByFilterRequest(
            request.Title,
            request.From,
            request.To,
            request.Page ?? 1,
            request.PageSize ?? 10);

        return await eventsService.GetEventsByFilterAsync(appServicesRequest, token);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventDto>> GetEventById(
        [FromRoute] Guid id, CancellationToken token) =>
        await eventsService.GetEventByIdAsync(id, token);

    [HttpPost]
    public async Task<IActionResult> CreateEvent(
        [FromBody] EventBaseRequest request, CancellationToken token)
    {
        var appServicesRequest = new AppServices.Data.Requests.Events.CreateEventRequest(
            request.Title, request.Description, request.StartAt, request.EndAt);
        var eventDto = await eventsService.CreateEventAsync(appServicesRequest, token);
        return CreatedAtAction(nameof(GetEventById), new { eventDto.Id }, eventDto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEvent(
        [FromRoute] Guid id, [FromBody] EventBaseRequest request, CancellationToken token)
    {
        var appServicesRequest = new AppServices.Data.Requests.Events.UpdateEventRequest(
            id, request.Title, request.Description, request.StartAt, request.EndAt);
        await eventsService.UpdateEventAsync(appServicesRequest, token);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEvent(
        [FromRoute] Guid id, CancellationToken token)
    {
        await eventsService.DeleteEventAsync(id, token);

        return NoContent();
    }
}