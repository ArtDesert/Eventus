using System.Net;
using System.Net.Http.Json;
using Eventus.Api.Contracts.Dto.Bookings;
using Eventus.Api.Contracts.Enums.Bookings;
using Eventus.Tests.Integration.Events;
using Eventus.WebApi.Controllers.WebRoutes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Eventus.Tests.Integration.Bookings.Tests;

public class BookingsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<BookingDto> WaitForBookingStatusAsync(
        Guid bookingId, BookingStatus expected, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        BookingDto? actual = null;

        while (DateTime.UtcNow < deadline)
        {
            actual = await _client.GetFromJsonAsync<BookingDto>(
                BookingsControllerWebRoutes.GetBookingByIdUrl(bookingId));

            if (actual?.Status == expected)
                return actual;

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        throw new TimeoutException(
            $"Booking with ID: {bookingId} did not reach '{expected}' within {timeout}. Last status: '{actual?.Status}'.");
    }

    #region CreateBooking

    [Fact]
    public async Task CreateBooking_NonExistentEvent_Returns404ProblemDetails()
    {
        // Arrange
        var randomEventId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync(
            BookingsControllerWebRoutes.CreateBookingUrl(randomEventId), content: null);

        // Assert
        await AssertProblemDetails.IsValid404ProblemDetailsAsync(response);
    }

    [Fact]
    public async Task CreateBooking_ExistingEvent_CreationAndAsyncConfirmationAreSuccessful()
    {
        // Arrange
        var @event = await EventsData.CreateAndAssertTestEventAsync(_client, EventsData.TestEventBaseRequest);
        var eventId = @event.Id;

        // Act
        var creationResponse = await _client.PostAsync(
            BookingsControllerWebRoutes.CreateBookingUrl(eventId), content: null);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, creationResponse.StatusCode);

        // Assert the pending booking.
        var pendingBooking = await creationResponse.Content.ReadFromJsonAsync<BookingDto>();
        Assert.NotNull(pendingBooking);
        var bookingId = pendingBooking.Id;
        Assert.Equal(eventId, pendingBooking.EventId);
        Assert.Equal(BookingStatus.Pending, pendingBooking.Status);
        Assert.NotEqual(default, pendingBooking.CreatedAt);
        Assert.Null(pendingBooking.ProcessedAt);

        // Wait for asynchronous booking confirmation. 
        var confirmedBooking = await WaitForBookingStatusAsync(
            bookingId, BookingStatus.Confirmed, TimeSpan.FromSeconds(30));

        // Check if the booking has updated.
        Assert.Equal(bookingId, confirmedBooking.Id);
        Assert.Equal(eventId, confirmedBooking.EventId);
        Assert.Equal(pendingBooking.CreatedAt, confirmedBooking.CreatedAt);
        Assert.NotNull(confirmedBooking.ProcessedAt);
        Assert.True(confirmedBooking.ProcessedAt >= confirmedBooking.CreatedAt);
    }

    [Fact]
    public async Task CreateBooking_CreateSomeBookingsForExistentEvent_AllBookingsAreUnique()
    {
        // Arrange
        var @event = await EventsData.CreateAndAssertTestEventAsync(_client, EventsData.TestEventBaseRequest);

        // Act
        var responses = await Task.WhenAll(
            Enumerable.Range(0, 10).Select(_ => _client.PostAsync(
                BookingsControllerWebRoutes.CreateBookingUrl(@event.Id), content: null)));

        // Assert
        var bookings = new List<BookingDto>(responses.Length);
        foreach (var response in responses)
        {
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            var booking = await response.Content.ReadFromJsonAsync<BookingDto>();
            Assert.NotNull(booking);
            Assert.Equal(@event.Id, booking.EventId);
            bookings.Add(booking);
        }

        Assert.Distinct(bookings.Select(b => b.Id));
    }

    #endregion

    #region GetBookingById

    [Fact]
    public async Task GetBookingById_NonExistentBooking_Returns404ProblemDetails()
    {
        // Arrange
        var randomBookingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync(BookingsControllerWebRoutes.GetBookingByIdUrl(randomBookingId));

        // Assert
        await AssertProblemDetails.IsValid404ProblemDetailsAsync(response);
    }

    [Fact]
    public async Task GetBookingById_ExistingBooking_ReturnsBooking()
    {
        // Arrange
        var @event = await EventsData.CreateAndAssertTestEventAsync(_client, EventsData.TestEventBaseRequest);
        var eventId = @event.Id;
        var creationResponse = await _client.PostAsync(
            BookingsControllerWebRoutes.CreateBookingUrl(eventId), content: null);
        Assert.Equal(HttpStatusCode.Accepted, creationResponse.StatusCode);
        var pendingBooking = await creationResponse.Content.ReadFromJsonAsync<BookingDto>();
        Assert.NotNull(pendingBooking);

        // Act
        var response = await _client.GetAsync(BookingsControllerWebRoutes.GetBookingByIdUrl(pendingBooking.Id));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var actualBooking = await response.Content.ReadFromJsonAsync<BookingDto>();
        Assert.NotNull(actualBooking);
        Assert.Equal(pendingBooking.Id, actualBooking.Id);
        Assert.Equal(pendingBooking.EventId, actualBooking.EventId);
    }

    #endregion
}