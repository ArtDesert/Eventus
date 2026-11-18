using System.Net.Http.Json;
using Eventus.AppServices.Services.Events;
using Eventus.Tests.Integration.Events.Fake;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Eventus.Tests.Integration.Events.Tests;

public class InternalServerErrorProblemDetailsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddScoped<IEventsService, EventsThrowingService>()))
        .CreateClient();

    [Fact]
    public async Task CreateEvent_EventsServiceThrowsException_Returns500ProblemDetails()
    {
        // Arrange
        // Act
        var response = await _client.PostAsJsonAsync(RequestUri.Events, TestDataFactory.CreateEventRequest);

        // Assert
        await AssertProblemDetails.IsValid500ProblemDetailsAsync(response);
    }

    [Fact]
    public async Task GetEventById_EventsServiceThrowsException_Returns500ProblemDetails()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"{RequestUri.Events}{id}");

        // Assert
        await AssertProblemDetails.IsValid500ProblemDetailsAsync(response);
    }

    [Fact]
    public async Task GetEventsByFilter_EventsServiceThrowsException_Returns500ProblemDetails()
    {
        // Arrange
        var query = string.Empty;
        // Act
        var response = await _client.GetAsync($"{RequestUri.Events}?{query}");

        // Assert
        await AssertProblemDetails.IsValid500ProblemDetailsAsync(response);
    }

    [Fact]
    public async Task UpdateEvent_EventsServiceThrowsException_Returns500ProblemDetails()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await _client.PutAsJsonAsync($"{RequestUri.Events}{id}", TestDataFactory.UpdateEventRequest);


        // Assert
        await AssertProblemDetails.IsValid500ProblemDetailsAsync(response);
    }

    [Fact]
    public async Task DeleteEvent_EventsServiceThrowsException_Returns500ProblemDetails()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"{RequestUri.Events}{id}");

        // Assert
        await AssertProblemDetails.IsValid500ProblemDetailsAsync(response);
    }
}