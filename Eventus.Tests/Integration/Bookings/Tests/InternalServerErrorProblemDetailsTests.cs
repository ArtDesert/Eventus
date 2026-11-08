using Eventus.AppServices.Services.Bookings;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Eventus.Tests.Integration.Bookings.Tests;

public class InternalServerErrorProblemDetailsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddScoped<IBookingsService, BookingsThrowingService>()))
        .CreateClient();

    [Fact]
    public async Task CreateBooking_BookingsServiceThrowsException_Returns500ProblemDetails()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"api/events/{eventId}/book", content: null);

        // Assert
        await AssertProblemDetails.IsValid500ProblemDetailsAsync(response);
    }

    [Fact]
    public async Task GetBookingById_BookingsServiceThrowsException_Returns500ProblemDetails()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"api/bookings/{bookingId}");

        // Assert
        await AssertProblemDetails.IsValid500ProblemDetailsAsync(response);
    }
}