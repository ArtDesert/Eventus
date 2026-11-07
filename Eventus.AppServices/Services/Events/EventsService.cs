using Eventus.Api.Contracts.Dto.Events;
using Eventus.AppServices.Data.Domain.Events;
using Eventus.AppServices.Data.Requests.Events;
using Eventus.AppServices.Exceptions;
using Eventus.AppServices.Repositories.Events;

namespace Eventus.AppServices.Services.Events;

internal class EventsService(IEventsRepository repository) : IEventsService
{
    public async Task<PaginatedEventsResultDto> GetEventsByFilterAsync(
        GetEventsByFilterRequest request, CancellationToken token)
    {
        var result = await repository.GetEventsByFilterAsync(request, token);

        return new PaginatedEventsResultDto
        {
            TotalCount = result.TotalCount,
            Items = result.Items
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    StartAt = e.StartAt,
                    EndAt = e.EndAt,
                })
                .ToArray(),
            Page = request.Page,
            ItemsCount = result.Items.Length,
        };
    }

    public async Task<EventDto> GetEventByIdAsync(Guid id, CancellationToken token)
    {
        var result = await repository.GetEventByIdAsync(id, token);

        if (result == null)
        {
            throw new NotFoundException($"Event with ID: {id} not found.");
        }

        return MapToEventDto(result);
    }

    public async Task<EventDto> CreateEventAsync(CreateEventRequest request, CancellationToken token)
    {
        var result = await repository.CreateEventAsync(request, token);
        return MapToEventDto(result);
    }

    public async Task UpdateEventAsync(UpdateEventRequest request, CancellationToken token)
    {
        var result = await repository.UpdateEventAsync(request, token);
        if (!result)
        {
            throw new NotFoundException($"Event with ID: {request.Id} not found.");
        }
    }

    public async Task DeleteEventAsync(Guid id, CancellationToken token)
    {
        var result = await repository.DeleteEventAsync(id, token);
        if (!result)
        {
            throw new NotFoundException($"Event with ID: {id} not found.");
        }
    }

    private static EventDto MapToEventDto(Event @event)
    {
        return new EventDto
        {
            Id = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt,
        };
    }
}