using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.Tests;

public static class AssertProblemDetails
{
    private static async Task<T> AssertProblemDetailsAsync<T>(
        HttpResponseMessage response, HttpStatusCode expectedStatusCode)
        where T : ProblemDetails
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<T>();
        Assert.NotNull(problem);
        Assert.Equal((int)expectedStatusCode, problem.Status);
        return problem;
    }

    public static async Task IsValid400ErrorAsync(HttpResponseMessage response, string expectedProblemField)
    {
        var problem = await AssertProblemDetailsAsync<ValidationProblemDetails>(response, HttpStatusCode.BadRequest);
        Assert.Contains(expectedProblemField, problem.Errors.Keys);
    }

    public static Task IsValid404ErrorAsync(HttpResponseMessage response) =>
        AssertProblemDetailsAsync<ProblemDetails>(response, HttpStatusCode.NotFound);

    public static async Task IsValid500ErrorAsync(HttpResponseMessage response)
    {
        var problem = await AssertProblemDetailsAsync<ProblemDetails>(response, HttpStatusCode.InternalServerError);
        Assert.Equal("An unexpected error occurred.", problem.Detail);
    }
}