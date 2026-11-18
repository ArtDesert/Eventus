using Eventus.AppServices.Data.Domain.Events;
using Eventus.AppServices.Repositories.Events;
using FluentAssertions;

namespace Eventus.Tests.Unit.Events.Repositories;

public class EventsRepositoryTests
{
    private readonly IEventsRepository _repository = new EventsInMemoryRepository([]);
    private readonly CancellationToken _token = CancellationToken.None;

    private async Task<Event> AddTestEvent()
    {
        var e = new Event(
            Guid.NewGuid(),
            "Test event",
            "Test description",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 18),
            10);
        return await _repository.CreateEventAsync(e, _token);
    }

    [Fact]
    public async Task CreateEvent_CorrectRequest_SavesEventWithAllProperties()
    {
        // Arrange
        var e = new Event(
            Guid.NewGuid(),
            "Test event",
            "Test description",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 18),
            10);

        // Act
        var createdEvent = await _repository.CreateEventAsync(e, _token);
        var @event = await _repository.GetEventByIdAsync(createdEvent.Id, _token);

        // Assert
        Assert.NotNull(createdEvent);
        createdEvent.Should().BeEquivalentTo(e);
        Assert.NotNull(@event);
        @event.Should().BeEquivalentTo(e);
    }

    [Fact]
    public async Task UpdateEvent_ExistingEvent_ReplacesAllProperties()
    {
        // Arrange
        var testEvent = await AddTestEvent();
        var updated = new Event(
            testEvent.Id,
            "New title",
            "New description",
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 18, 10, 0, 0),
            10);

        // Act
        var isUpdated = await _repository.TryUpdateEventAsync(testEvent, updated, _token);
        var updatedEvent = await _repository.GetEventByIdAsync(testEvent.Id, _token);

        // Assert
        Assert.True(isUpdated);
        updatedEvent.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task UpdateEvent_NonExistentEvent_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updated = new Event(
            id,
            "New title",
            "New description",
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 18, 10, 0, 0),
            10);

        // Act
        var isUpdated = await _repository.TryUpdateEventAsync(current: null!, updated, _token);
        var nonExistentEvent = await _repository.GetEventByIdAsync(id, _token);

        // Assert
        Assert.False(isUpdated);
        Assert.Null(nonExistentEvent);
    }

    [Fact]
    public async Task DeleteEvent_ExistingEvent_DeletesEventAndReturnsTrue()
    {
        // Arrange
        var testEvent = await AddTestEvent();

        // Act
        var isDeleted = await _repository.DeleteEventAsync(testEvent.Id, _token);
        var deletedEvent = await _repository.GetEventByIdAsync(testEvent.Id, _token);

        // Assert
        Assert.True(isDeleted);
        Assert.Null(deletedEvent);
    }

    [Fact]
    public async Task DeleteEvent_NonExistentEvent_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var isDeleted = await _repository.DeleteEventAsync(id, _token);
        var nonExistentEvent = await _repository.GetEventByIdAsync(id, _token);

        // Assert 
        Assert.False(isDeleted);
        Assert.Null(nonExistentEvent);
    }

    [Fact]
    public async Task GetEventById_ExistingEvent_ReturnsEvent()
    {
        // Arrange
        var testEvent = await AddTestEvent();

        // Act
        var @event = await _repository.GetEventByIdAsync(testEvent.Id, _token);

        // Assert
        Assert.NotNull(@event);
        @event.Should().BeEquivalentTo(testEvent);
    }

    [Fact]
    public async Task GetEventById_NonExistentEvent_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var @event = await _repository.GetEventByIdAsync(id, _token);

        // Assert
        Assert.Null(@event);
    }
}