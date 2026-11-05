using System.Net;
using System.Net.Http.Json;
using Eventus.Api.Contracts.Dto.Events;
using Eventus.Api.Contracts.Requests.Events;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Eventus.Tests.Integration.Events.Tests;

public class EventsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static EventBaseRequest TestEventBaseRequest => new EventBaseRequest
    {
        Title = "Test",
        Description = "Description",
        StartAt = new DateTime(2000, 1, 1),
        EndAt = new DateTime(2000, 1, 1)
    };
    
    private async Task<EventDto> CreateTestEventAsync(EventBaseRequest request)
    {
        var creationResponse = await _client.PostAsJsonAsync(RequestUri.Events, request);
        
        // Check the creation before the next action.
        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);
        var createdEvent = await creationResponse.Content.ReadFromJsonAsync<EventDto>();
        Assert.NotNull(createdEvent);
        return createdEvent;
    }

    #region GetEvent

    [Fact]
    public async Task GetEventById_NonExistentEvent_Returns404ProblemDetails()
    {
        // Arrange
        var randomId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"{RequestUri.Events}{randomId}");

        // Assert
        await AssertProblemDetails.IsValid404ProblemDetailsAsync(response);
    }

    [Fact]
    public async Task GetEventById_ExistingEvent_ReturnsEvent()
    {
        // Arrange
        var createdEvent = await CreateTestEventAsync(TestEventBaseRequest);
        var id = createdEvent.Id;

        // Act
        var response = await _client.GetAsync($"{RequestUri.Events}{id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var @event = await response.Content.ReadFromJsonAsync<EventDto>();
        Assert.NotNull(@event);
        Assert.Equal(id, @event.Id);
        @event.Should().BeEquivalentTo(TestEventBaseRequest);
    }

    #endregion

    #region UpdateEvent

    [Fact]
    public async Task UpdateEvent_NonExistentEvent_Returns404ProblemDetails()
    {
        // Arrange
        var randomId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsJsonAsync($"{RequestUri.Events}{randomId}", TestEventBaseRequest);

        // Assert
        await AssertProblemDetails.IsValid404ProblemDetailsAsync(response);
    }

    [Fact]
    public async Task UpdateEvent_ExistingEvent_UpdateIsSuccessful()
    {
        // Arrange
        var createdEvent = await CreateTestEventAsync(TestEventBaseRequest);
        var id = createdEvent.Id;
        var updateRequest = new EventBaseRequest
        {
            Title = "New title",
            Description = "New description",
            StartAt = new DateTime(2020, 1, 2),
            EndAt = new DateTime(2021, 4, 3),
        };
        // Check that the creation and update requests are different.
        updateRequest.Should().NotBeEquivalentTo(TestEventBaseRequest);

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"{RequestUri.Events}{id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        var gettingResponseAfterUpdate = await _client.GetAsync($"{RequestUri.Events}{id}");
        
        Assert.Equal(HttpStatusCode.OK, gettingResponseAfterUpdate.StatusCode);
        var @event = await gettingResponseAfterUpdate.Content.ReadFromJsonAsync<EventDto>();
        Assert.NotNull(@event);
        Assert.Equal(id, @event.Id);
        @event.Should().BeEquivalentTo(updateRequest);
    }

    #endregion

    #region DeleteEvent

    [Fact]
    public async Task DeleteEvent_NonExistentEvent_Returns404ProblemDetails()
    {
        // Arrange
        var randomId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"{RequestUri.Events}{randomId}");

        // Assert
        await AssertProblemDetails.IsValid404ProblemDetailsAsync(response);
    }

    [Fact]
    public async Task DeleteEvent_ExistingEvent_DeletionIsSuccessful()
    {
        // Arrange
        var createdEvent = await CreateTestEventAsync(TestEventBaseRequest);
        var id = createdEvent.Id;

        // Act
        var response = await _client.DeleteAsync($"{RequestUri.Events}{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var gettingResponseAfterDeletion = await _client.GetAsync($"{RequestUri.Events}{id}");
        Assert.Equal(HttpStatusCode.NotFound, gettingResponseAfterDeletion.StatusCode);
    }

    #endregion
}