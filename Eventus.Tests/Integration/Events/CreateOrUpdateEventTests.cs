using System.Net;
using System.Net.Http.Json;
using System.Text;
using Eventus.Api.Contracts.Requests.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Eventus.Tests.Integration.Events;

public class CreateOrUpdateEventTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly DateTime StartDate = new(2026, 1, 1, 10, 0, 0);
    private static readonly Guid EventId = Guid.Parse("75617c6b-1b3a-46ee-ad50-5135e8c894b7");

    private static EventBaseRequest BuildRequest(
        string title = "Test event",
        string? description = null,
        DateTime? startAt = null,
        DateTime? endAt = null) => new()
    {
        Title = title,
        Description = description,
        StartAt = startAt ?? StartDate,
        EndAt = endAt ?? StartDate.AddHours(5),
    };

    public static TheoryData<string, EventBaseRequest, string> IncorrectRequests => new()
    {
        {
            "EmptyTitle",
            BuildRequest(title: ""),
            nameof(EventBaseRequest.Title)
        },
        {
            "TitleIsWhiteSpaceOnly",
            BuildRequest(title: "       "),
            nameof(EventBaseRequest.Title)
        },
        {
            "IncorrectDateOrder_ProblemFieldIsStartAt",
            BuildRequest(endAt: StartDate.AddHours(-1)),
            nameof(EventBaseRequest.StartAt)
        },
        {
            "IncorrectDateOrder_ProblemFieldIsEndAt",
            BuildRequest(endAt: StartDate.AddHours(-1)),
            nameof(EventBaseRequest.EndAt)
        },
    };

    public static TheoryData<string, string, string> IncorrectJsons => new()
    {
        {
            "TitleIsNull",
            """{ "startAt": "2026-01-01T10:00:00", "endAt": "2026-01-01T15:00:00" }""",
            nameof(EventBaseRequest.Title)
        },
        {
            "StartDateIsDefault",
            """{ "title": "Test event", "endAt": "2026-01-01T15:00:00" }""",
            nameof(EventBaseRequest.StartAt)
        },
        {
            "EndDateIsDefault",
            """{ "title": "Test event", "startAt": "2026-01-01T10:00:00" }""",
            nameof(EventBaseRequest.EndAt)
        },
        {
            "DatesAreDefault_ProblemFieldIsStartAt",
            """{ "title": "Test event" }""",
            nameof(EventBaseRequest.StartAt)
        },
        {
            "DatesAreDefault_ProblemFieldIsEndAt",
            """{ "title": "Test event" }""",
            nameof(EventBaseRequest.EndAt)
        },
        {
            "StartDateIsString",
            """{ "title": "Test event", "startAt": "string", "endAt": "2026-01-01T15:00:00" }""",
            "$.startAt"
        },
    };

    public static TheoryData<string, EventBaseRequest> CorrectRequests => new()
    {
        { "FullRequestWithoutDescription", BuildRequest() },
        { "FullRequestWithDescription", BuildRequest(description: "Test description") },
        { "DatesAreEqual", BuildRequest(startAt:StartDate, endAt: StartDate) },
    };

    #region CreateEvent

    [Theory]
    [MemberData(nameof(IncorrectRequests))]
    public async Task CreateEvent_InvalidRequest_Returns400WithProblemKey(
        string _, EventBaseRequest request, string field)
    {
        // Arrange
        // Act
        var response = await _client.PostAsJsonAsync("api/events", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains(field, problem!.Errors.Keys);
    }

    [Theory]
    [MemberData(nameof(IncorrectJsons))]
    public async Task CreateEvent_IncorrectJsonAsBody_Returns400WithProblemKey(
        string _, string json, string field)
    {
        // Arrange
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("api/events", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains(field, problem!.Errors.Keys);
    }

    [Theory]
    [MemberData(nameof(CorrectRequests))]
    public async Task CreateEvent_ValidationPassed_Returns201(
        string _, EventBaseRequest request)
    {
        // Arrange
        // Act
        var response = await _client.PostAsJsonAsync("api/events", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    #endregion

    #region UpdateEvent

    [Theory]
    [MemberData(nameof(IncorrectRequests))]
    public async Task UpdateEvent_InvalidRequest_Returns400WithProblemKey(
        string _, EventBaseRequest request, string field)
    {
        // Arrange
        // Act
        var response = await _client.PutAsJsonAsync($"api/events/{EventId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains(field, problem!.Errors.Keys);
    }

    [Theory]
    [MemberData(nameof(IncorrectJsons))]
    public async Task UpdateEvent_IncorrectJsonAsBody_Returns400(
        string _, string json, string field)
    {
        // Arrange
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"api/events/{EventId}", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains(field, problem!.Errors.Keys);
    }

    [Theory]
    [MemberData(nameof(CorrectRequests))]
    public async Task UpdateEvent_ValidationPassed_Returns404(
        string _, EventBaseRequest request)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await _client.PutAsJsonAsync($"api/events/{id}", request);

        // Assert
        // We use a random ID so event is non-existent (404).
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
}