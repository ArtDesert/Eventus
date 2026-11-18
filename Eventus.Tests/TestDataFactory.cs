using System.Net;
using System.Net.Http.Json;
using Eventus.Api.Contracts.Dto.Events;
using Eventus.Api.Contracts.Requests.Events;
using Eventus.AppServices.Data.Domain.Bookings;
using Eventus.AppServices.Data.Domain.Events;

namespace Eventus.Tests;

public static class TestDataFactory
{
    public static DateTime StartDate { get; } = DateTime.UtcNow.AddHours(24);
    public static DateTime EndDate { get; } = StartDate.AddHours(24);

    public static CreateEventRequest CreateEventRequest { get; } = new()
    {
        Title = "Test",
        Description = "Description",
        StartAt = StartDate,
        EndAt = EndDate,
        TotalSeats = 10
    };

    public static CreateEventRequest BuildRequestToCreateEvent(
        string title = "Test",
        string? description = "Description",
        DateTime? startAt = null,
        DateTime? endAt = null,
        int totalSeats = 10) => new CreateEventRequest
        {
            Title = title,
            Description = description,
            StartAt = startAt ?? StartDate,
            EndAt = endAt ?? EndDate,
            TotalSeats = totalSeats
        };

    public static UpdateEventRequest UpdateEventRequest { get; } = new()
    {
        Title = "Test",
        Description = "Description",
        StartAt = StartDate,
        EndAt = EndDate
    };

    public static Guid EventId { get; } = Guid.Parse("696ab98f-8ed7-4882-bd08-253743a18d04");
    public static Guid BookingId { get; } = Guid.Parse("bc1b1f5b-6ba6-423b-85d8-d2c93ee711e5");

    public static Event Event { get; } = new(
        EventId,
        "Test",
        "Description",
        StartDate,
        EndDate,
        10
    );

    public static Event BuildEvent(
        Guid? eventId = null,
        string title = "Test",
        string? description = "Description",
        DateTime? startAt = null,
        DateTime? endAt = null,
        int totalSeats = 10) =>
        new(
            eventId ?? EventId,
            title,
            description,
            startAt ?? StartDate,
            endAt ?? EndDate,
            totalSeats);

    public static Booking PendingBooking { get; } = new(BookingId, EventId);

    public static async Task<EventDto> CreateAndAssertTestEventAsync(
        HttpClient client, CreateEventRequest request)
    {
        var response = await client.PostAsJsonAsync(RequestUri.Events, request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdEvent = await response.Content.ReadFromJsonAsync<EventDto>();
        Assert.NotNull(createdEvent);
        return createdEvent;
    }
}