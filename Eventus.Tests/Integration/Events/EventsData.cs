using System.Net;
using System.Net.Http.Json;
using Eventus.Api.Contracts.Dto.Events;
using Eventus.Api.Contracts.Requests.Events;

namespace Eventus.Tests.Integration.Events;

public static class EventsData
{
    private static readonly DateTime StartDate = DateTime.UtcNow.AddHours(24);

    public static EventBaseRequest TestEventBaseRequest { get; } = new()
    {
        Title = "Test",
        Description = "Description",
        StartAt = StartDate,
        EndAt = StartDate.AddHours(24)
    };

    public static async Task<EventDto> CreateAndAssertTestEventAsync(
        HttpClient client, EventBaseRequest request)
    {
        var response = await client.PostAsJsonAsync(RequestUri.Events, request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdEvent = await response.Content.ReadFromJsonAsync<EventDto>();
        Assert.NotNull(createdEvent);
        return createdEvent;
    }
}