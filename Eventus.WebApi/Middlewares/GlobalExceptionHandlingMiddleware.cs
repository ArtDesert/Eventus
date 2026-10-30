using Eventus.AppServices.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.WebApi.Middlewares;

public class GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            var hasResponseStarted = await HandleException(httpContext, ex);
            if (hasResponseStarted)
            {
                throw;
            }
        }
    }

    private async Task<bool> HandleException(HttpContext httpContext, Exception ex)
    {
        var problemDetails = MapExceptionToProblemDetails(ex);
        if (problemDetails.Status >= 500)
        {
            logger.LogError(
                ex,
                "Internal server error. Method={Method}, Path={Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation("Request failed with {StatusCode}: {Message}", problemDetails.Status, ex.Message);
        }

        if (httpContext.Response.HasStarted)
        {
            return true;
        }

        httpContext.Response.Clear();
        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext, ProblemDetails = problemDetails, Exception = ex
        });

        return false;
    }

    private static ProblemDetails MapExceptionToProblemDetails(Exception ex)
        => ex switch
        {
            DomainValidationException e => new ValidationProblemDetails(
                new Dictionary<string, string[]> { [e.ProblemFieldName] = [e.Message] })
            {
                Status = StatusCodes.Status400BadRequest
            },
            NotFoundException e => new ProblemDetails { Status = StatusCodes.Status404NotFound, Detail = e.Message },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError, Detail = "An unexpected error occurred."
            }
        };
}