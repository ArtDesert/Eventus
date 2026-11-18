using Eventus.Api.Contracts.Dto.Events;
using Eventus.AppServices.Data;
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
            Items = [.. result.Items.Select(MapToEventDto)],
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
        var @event = new Event(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.StartAt,
            request.EndAt,
            request.TotalSeats);
        var result = await repository.CreateEventAsync(@event, token);
        return MapToEventDto(result);
    }

    public async Task UpdateEventAsync(UpdateEventRequest request, CancellationToken token)
    {
        await EventsBookingsSemaphore.Semaphore.WaitAsync(token);
        try
        {
            var @event = await repository.GetEventByIdAsync(request.Id, token);
            if (@event == null)
            {
                throw new NotFoundException($"Event with ID: {request.Id} not found.");
            }

            var updatedEvent = @event.Update(request.Title, request.Description, request.StartAt, request.EndAt);
            if (!await repository.TryUpdateEventAsync(@event, updatedEvent, token))
            {
                throw new ConcurrentEventModificationException(
                    $"Event {@event.Id} was modified concurrently.");
            }
        }
        finally
        {
            EventsBookingsSemaphore.Semaphore.Release();
        }
    }

    public async Task DeleteEventAsync(Guid id, CancellationToken token)
    {
        await EventsBookingsSemaphore.Semaphore.WaitAsync(token);
        try
        {
            var result = await repository.DeleteEventAsync(id, token);
            if (!result)
            {
                throw new NotFoundException($"Event with ID: {id} not found.");
            }
        }
        finally
        {
            EventsBookingsSemaphore.Semaphore.Release();
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
            TotalSeats = @event.TotalSeats,
            AvailableSeats = @event.AvailableSeats
        };
    }
}