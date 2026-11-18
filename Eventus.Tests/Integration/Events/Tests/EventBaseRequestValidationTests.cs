using System.Net;
using System.Net.Http.Json;
using System.Text;
using Eventus.Api.Contracts.Requests.Events;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Eventus.Tests.Integration.Events.Tests;

public class EventBaseRequestValidationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    public static TheoryData<string, EventBaseRequest, string> IncorrectRequests => new()
    {
        {
            "EmptyTitle",
            TestDataFactory.BuildRequestToCreateEvent(title: ""),
            nameof(EventBaseRequest.Title)
        },
        {
            "TitleIsWhiteSpaceOnly",
            TestDataFactory.BuildRequestToCreateEvent(title: "       "),
            nameof(EventBaseRequest.Title)
        },
        {
            "IncorrectDateOrder_ProblemFieldIsStartAt",
            TestDataFactory.BuildRequestToCreateEvent(endAt: TestDataFactory.StartDate.AddHours(-1)),
            nameof(EventBaseRequest.StartAt)
        },
        {
            "IncorrectDateOrder_ProblemFieldIsEndAt",
            TestDataFactory.BuildRequestToCreateEvent(endAt: TestDataFactory.StartDate.AddHours(-1)),
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

    public static TheoryData<string, CreateEventRequest> CorrectRequests => new()
    {
        { "FullRequestWithoutDescription", TestDataFactory.BuildRequestToCreateEvent() },
        { "FullRequestWithDescription", TestDataFactory.BuildRequestToCreateEvent(description: "Test description") },
        { "DatesAreEqual", TestDataFactory.BuildRequestToCreateEvent(startAt: TestDataFactory.StartDate, endAt: TestDataFactory.StartDate) },
    };

    #region CreateEvent

    [Theory]
    [MemberData(nameof(IncorrectRequests))]
    public async Task CreateEvent_InvalidRequest_Returns400ProblemDetails(
        string _, EventBaseRequest request, string field)
    {
        // Arrange
        // Act
        var response = await _client.PostAsJsonAsync(RequestUri.Events, request);

        // Assert
        await AssertProblemDetails.IsValid400ProblemDetailsAsync(response, field);
    }

    [Theory]
    [MemberData(nameof(IncorrectJsons))]
    public async Task CreateEvent_IncorrectJsonAsBody_Returns400ProblemDetails(
        string _, string json, string field)
    {
        // Arrange
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(RequestUri.Events, content);

        // Assert
        await AssertProblemDetails.IsValid400ProblemDetailsAsync(response, field);
    }

    [Theory]
    [MemberData(nameof(CorrectRequests))]
    public async Task CreateEvent_ValidationPassed_Returns201(
        string _, CreateEventRequest request)
    {
        // Arrange
        // Act
        var response = await _client.PostAsJsonAsync(RequestUri.Events, request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    #endregion

    #region UpdateEvent

    [Theory]
    [MemberData(nameof(IncorrectRequests))]
    public async Task UpdateEvent_InvalidRequest_Returns400ProblemDetails(
        string _, EventBaseRequest request, string field)
    {
        // Arrange
        // Act
        var response = await _client.PutAsJsonAsync($"{RequestUri.Events}{TestDataFactory.EventId}", request);

        // Assert
        await AssertProblemDetails.IsValid400ProblemDetailsAsync(response, field);
    }

    [Theory]
    [MemberData(nameof(IncorrectJsons))]
    public async Task UpdateEvent_IncorrectJsonAsBody_Returns400ProblemDetails(
        string _, string json, string field)
    {
        // Arrange
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"{RequestUri.Events}{TestDataFactory.EventId}", content);

        // Assert
        await AssertProblemDetails.IsValid400ProblemDetailsAsync(response, field);
    }

    [Theory]
    [MemberData(nameof(CorrectRequests))]
    public async Task UpdateEvent_ValidationPassed_Returns404ProblemDetails(
        string _, EventBaseRequest request)
    {
        // Arrange
        var randomId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsJsonAsync($"{RequestUri.Events}{randomId}", request);

        // Assert
        // We use a random ID, so event is non-existent (404).
        await AssertProblemDetails.IsValid404ProblemDetailsAsync(response);
    }

    #endregion
}