using System.Net;
using System.Net.Http.Json;
using Eventus.Api.Contracts.Requests.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Eventus.Tests.Integration.Events;

public class GetEventsByFilterTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    public static TheoryData<string, string, string> IncorrectQueries => new()
    {
        {
            "PageIsZero",
            "title=Test event&from=2026-01-01T10:00:00&to=2026-01-01T15:00:00&page=0&pageSize=10",
            nameof(GetEventsByFilterRequest.Page)
        },
        {
            "PageIs101",
            "title=Test event&from=2026-01-01T10:00:00&to=2026-01-01T15:00:00&page=101&pageSize=10",
            nameof(GetEventsByFilterRequest.Page)
        },
        {
            "PageSizeIsZero",
            "title=Test event&from=2026-01-01T10:00:00&to=2026-01-01T15:00:00&page=1&pageSize=0",
            nameof(GetEventsByFilterRequest.PageSize)
        },
        {
            "PageSizeIs101",
            "title=Test event&from=2026-01-01T10:00:00&to=2026-01-01T15:00:00&page=1&pageSize=101",
            nameof(GetEventsByFilterRequest.PageSize)
        },
        {
            "IncorrectDateTimeOrder",
            "title=Test event&from=2026-01-01T10:00:00&to=2026-01-01T09:00:00&page=1&pageSize=10",
            nameof(GetEventsByFilterRequest.From)
        },
        {
            "IncorrectDateOrder",
            "title=Test event&from=2026-05-05T00:00:00&to=2026-04-04T00:00:00&page=1&pageSize=10",
            nameof(GetEventsByFilterRequest.From)
        },
        {
            "DatesAreString",
            "title=Test event&from=string1&to=string2&page=1&pageSize=10",
            nameof(GetEventsByFilterRequest.From)
        },
        {
            "PageIsString",
            "title=Test event&from=2026-01-01T10:00:00&to=2026-01-01T15:00:00&page=one&pageSize=10",
            nameof(GetEventsByFilterRequest.Page)
        },
        {
            "PageSizeIsString",
            "title=Test event&from=2026-01-01T10:00:00&to=2026-01-01T15:00:00&page=1&pageSize=ten",
            nameof(GetEventsByFilterRequest.PageSize)
        }
    };

    public static TheoryData<string, string> CorrectQueries => new()
    {
        {
            "EmptyQuery", ""
        },
        {
            "NullTitle", "title="
        },
        {
            "WhiteSpaceTitle", "title=         "
        },
        {
            "PageIs1", "page=1"
        },
        {
            "PageIs100", "page=100"
        },
        {
            "PageSizeIs1", "pageSize=1"
        },
        {
            "PageSizeIs100", "pageSize=100"
        },
        {
            "OnlyFrom", "From=2026-01-01T01:00:00"
        },
        {
            "OnlyTo", "To=2028-01-01T01:00:00"
        },
        {
            "DatesAreEqual", "From=2028-01-01T22:10:00&To=2028-01-01T22:10:00"
        },
        {
            "CorrectDateTimeOrder", "From=2028-01-01T22:10:00&To=2028-01-02T05:20:00"
        },
    };

    [Theory]
    [MemberData(nameof(IncorrectQueries))]
    public async Task GetEventsByFilter_IncorrectQuery_Returns400WithProblemKey(
        string _, string query,  string field)
    {
        // Arrange
        // Act
        var response = await _client.GetAsync($"/api/events?{query}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains(field, problem!.Errors.Keys);
    }
    
    [Theory]
    [MemberData(nameof(CorrectQueries))]
    public async Task GetEventsByFilter_ValidationPassed_Returns200(string _, string query)
    {
        // Arrange
        // Act
        var response = await _client.GetAsync($"/api/events?{query}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}