using System.Net;
using System.Net.Http.Json;
using Eventus.Api.Contracts.Dto.Bookings;
using Eventus.Api.Contracts.Dto.Events;
using Eventus.Api.Contracts.Enums.Bookings;
using Eventus.Api.Contracts.Requests.Events;
using Eventus.WebApi.Controllers.WebRoutes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Eventus.Tests.Integration.Bookings.Tests;

public class BookingsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory
        .WithWebHostBuilder(b => b.UseEnvironment("Test"))
        .CreateClient();

    private async Task<BookingDto> WaitForBookingProcessingAsync(Guid bookingId, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        BookingDto? actual = null;

        while (DateTime.UtcNow < deadline)
        {
            actual = await _client.GetFromJsonAsync<BookingDto>(
                BookingsControllerWebRoutes.GetBookingByIdUrl(bookingId));

            if (actual?.Status is BookingStatus.Confirmed or BookingStatus.Rejected)
                return actual;

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        throw new TimeoutException(
            $"Booking {bookingId} did not process within {timeout}. Last status: '{actual?.Status}'.");
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
        var sourceEvent = await TestDataFactory.CreateAndAssertTestEventAsync(
            _client, TestDataFactory.CreateEventRequest);
        var eventId = sourceEvent.Id;
        var oldAvailableSeats = sourceEvent.AvailableSeats;

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
        var confirmedBooking = await WaitForBookingProcessingAsync(
            bookingId, TimeSpan.FromSeconds(30));

        // Check if the booking has updated.
        Assert.Equal(bookingId, confirmedBooking.Id);
        Assert.Equal(eventId, confirmedBooking.EventId);
        Assert.Equal(BookingStatus.Confirmed, confirmedBooking.Status);
        Assert.Equal(pendingBooking.CreatedAt, confirmedBooking.CreatedAt);
        Assert.NotNull(confirmedBooking.ProcessedAt);
        Assert.True(confirmedBooking.ProcessedAt >= confirmedBooking.CreatedAt);

        var response = await _client.GetAsync($"{RequestUri.Events}{eventId}");
        var updatedEvent = await response.Content.ReadFromJsonAsync<EventDto>();
        Assert.NotNull(updatedEvent);
        Assert.Equal(oldAvailableSeats - 1, updatedEvent.AvailableSeats);
    }

    [Fact]
    public async Task CreateBooking_CreateSomeBookingsForExistentEvent_AllBookingsAreUnique()
    {
        // Arrange
        const int totalSeats = 10;
        var @event = await TestDataFactory.CreateAndAssertTestEventAsync(
            _client, TestDataFactory.BuildRequestToCreateEvent(totalSeats: totalSeats));

        // Act
        var responses = await Task.WhenAll(
            Enumerable.Range(0, totalSeats).Select(_ => _client.PostAsync(
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
        var updatedEvent = await (await _client.GetAsync($"{RequestUri.Events}{@event.Id}")).Content.ReadFromJsonAsync<EventDto>();
        Assert.NotNull(updatedEvent);
        Assert.Equal(0, updatedEvent.AvailableSeats);
    }

    [Fact]
    public async Task CreateBooking_OverBooking_CorrectOverBookingProcessing()
    {
        // Arrange
        const int totalSeats = 5;
        const int overBookingCount = 15;
        var @event = await TestDataFactory.CreateAndAssertTestEventAsync(
            _client, TestDataFactory.BuildRequestToCreateEvent(totalSeats: totalSeats));

        // Act
        var responses = await Task.WhenAll(
            Enumerable.Range(0, totalSeats + overBookingCount).Select(_ => _client.PostAsync(
                BookingsControllerWebRoutes.CreateBookingUrl(@event.Id), content: null)));

        // Assert
        var responsesWith409Code =
            responses.Where(response => response.StatusCode == HttpStatusCode.Conflict).ToArray();
        var responsesWith202Code = responses.Where(response => response.StatusCode == HttpStatusCode.Accepted).ToArray();

        Assert.Equal(overBookingCount, responsesWith409Code.Length);
        Assert.Equal(totalSeats, responsesWith202Code.Length);

        var bookingsIds = await Task.WhenAll(
            responsesWith202Code.Select(response => response.Content.ReadFromJsonAsync<BookingDto>()));

        var bookingProcessingResult = await Task.WhenAll(
            bookingsIds.Select(b => WaitForBookingProcessingAsync(b!.Id, TimeSpan.FromSeconds(30))));

        Assert.All(bookingProcessingResult, b => Assert.Equal(BookingStatus.Confirmed, b.Status));
        var response = await _client.GetAsync($"{RequestUri.Events}{@event.Id}");
        var updatedEvent = await response.Content.ReadFromJsonAsync<EventDto>();
        Assert.NotNull(updatedEvent);
        Assert.Equal(0, updatedEvent.AvailableSeats);
    }

    [Fact]
    public async Task CreateBooking_ConcurrentWithEventUpdates_SeatsAreNotLost()
    {
        // Arrange
        const int totalSeats = 20;
        const int bookingsCount = 10;
        const int updatesCount = 10;
        var @event = await TestDataFactory.CreateAndAssertTestEventAsync(
            _client, TestDataFactory.BuildRequestToCreateEvent(totalSeats: totalSeats));

        // Act
        var bookingTasks = Enumerable.Range(0, bookingsCount).Select(_ => _client.PostAsync(
            BookingsControllerWebRoutes.CreateBookingUrl(@event.Id), content: null));
        var updateTasks = Enumerable.Range(0, updatesCount).Select(i => _client.PutAsJsonAsync(
            $"{RequestUri.Events}{@event.Id}",
            new UpdateEventRequest
            {
                Title = $"Updated title {i}",
                Description = "Updated description",
                StartAt = TestDataFactory.StartDate,
                EndAt = TestDataFactory.EndDate
            }));
        var responses = await Task.WhenAll(bookingTasks.Concat(updateTasks));

        // Assert
        Assert.Equal(bookingsCount, responses.Count(r => r.StatusCode == HttpStatusCode.Accepted));
        Assert.Equal(updatesCount, responses.Count(r => r.StatusCode == HttpStatusCode.NoContent));

        var actualEvent = await _client.GetFromJsonAsync<EventDto>($"{RequestUri.Events}{@event.Id}");
        Assert.NotNull(actualEvent);
        Assert.StartsWith("Updated title", actualEvent.Title);
        Assert.Equal(totalSeats, actualEvent.TotalSeats);
        Assert.Equal(totalSeats - bookingsCount, actualEvent.AvailableSeats);
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
        var @event =
            await TestDataFactory.CreateAndAssertTestEventAsync(_client,
                TestDataFactory.CreateEventRequest);
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