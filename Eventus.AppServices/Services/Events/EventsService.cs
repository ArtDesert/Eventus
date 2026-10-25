using Eventus.Api.Contracts.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;
using Eventus.AppServices.Exceptions;
using Eventus.AppServices.Repositories.Events;

namespace Eventus.AppServices.Services.Events;

internal class EventsService(IEventsRepository repository) : IEventsService
{
    public PaginatedEventsResultDto GetEventsByFilter(GetEventsByFilterRequest request)
    {
        var result = repository.GetEventsByFilter(request);

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

    public EventDto? GetEventById(Guid id)
    {
        var result = repository.GetEventById(id);

        if (result == null)
        {
            throw new NotFoundException($"Event with ID: {id} not found.");
        }

        return new EventDto
        {
            Id = result.Id,
            Title = result.Title,
            Description = result.Description,
            StartAt = result.StartAt,
            EndAt = result.EndAt,
        };
    }

    public Guid CreateEvent(CreateEventRequest request)
    {
        return repository.CreateEvent(request);
    }

    public void UpdateEvent(UpdateEventRequest request)
    {
        var result = repository.UpdateEvent(request);
        if (!result)
        {
            throw new NotFoundException($"Event with ID: {request.Id} not found.");
        }
    }

    public void DeleteEvent(Guid id)
    {
        var result = repository.DeleteEvent(id);
        if (!result)
        {
            throw new NotFoundException($"Event with ID: {id} not found.");
        }
    }
}