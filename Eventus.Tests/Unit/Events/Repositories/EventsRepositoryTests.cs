using Eventus.AppServices.Data.Domain.Events;
using Eventus.AppServices.Data.Requests.Events;
using Eventus.AppServices.Repositories.Events;
using FluentAssertions;

namespace Eventus.Tests.Unit.Events.Repositories;

public class EventsRepositoryTests
{
    private readonly IEventsRepository _repository = new EventsInMemoryRepository([]);

    private Event AddTestEvent()
    {
        var request = new CreateEventRequest(
            "Test event",
            "Test description",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 18));
        return _repository.CreateEvent(request);
    }

    [Fact]
    public void CreateEvent_CorrectRequest_SavesEventWithAllProperties()
    {
        // Arrange
        var request = new CreateEventRequest(
            "Test event",
            "Test description",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 18));

        // Act
        var createdEvent = _repository.CreateEvent(request);
        var @event = _repository.GetEventById(createdEvent.Id);

        // Assert
        Assert.NotNull(createdEvent);
        createdEvent.Should().BeEquivalentTo(request);
        Assert.NotNull(@event);
        @event.Should().BeEquivalentTo(request);
    }

    [Fact]
    public void UpdateEvent_ExistingEvent_ReplacesAllProperties()
    {
        // Arrange
        var testEvent = AddTestEvent();
        var updateRequest = new UpdateEventRequest(
            testEvent.Id,
            "New title",
            "New description",
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 18, 10, 0, 0));

        // Act
        var isUpdated = _repository.UpdateEvent(updateRequest);
        var updatedEvent = _repository.GetEventById(testEvent.Id);

        // Assert
        Assert.True(isUpdated);
        updatedEvent.Should().BeEquivalentTo(updateRequest);
    }

    [Fact]
    public void UpdateEvent_NonExistentEvent_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateRequest = new UpdateEventRequest(
            id,
            "New title",
            "New description",
            new DateTime(2026, 1, 1, 10, 0, 0),
            new DateTime(2026, 1, 18, 10, 0, 0));

        // Act
        var isUpdated = _repository.UpdateEvent(updateRequest);
        var nonExistentEvent = _repository.GetEventById(id);

        // Assert
        Assert.False(isUpdated);
        Assert.Null(nonExistentEvent);
    }

    [Fact]
    public void DeleteEvent_ExistingEvent_DeletesEventAndReturnsTrue()
    {
        // Arrange
        var testEvent = AddTestEvent();

        // Act
        var isDeleted = _repository.DeleteEvent(testEvent.Id);
        var deletedEvent = _repository.GetEventById(testEvent.Id);

        // Assert
        Assert.True(isDeleted);
        Assert.Null(deletedEvent);
    }

    [Fact]
    public void DeleteEvent_NonExistentEvent_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var isDeleted = _repository.DeleteEvent(id);
        var nonExistentEvent = _repository.GetEventById(id);

        // Assert 
        Assert.False(isDeleted);
        Assert.Null(nonExistentEvent);
    }

    [Fact]
    public void GetEventById_ExistingEvent_ReturnsEvent()
    {
        // Arrange
        var testEvent = AddTestEvent();

        // Act
        var @event = _repository.GetEventById(testEvent.Id);

        // Assert
        Assert.NotNull(@event);
        @event.Should().BeEquivalentTo(testEvent);
    }

    [Fact]
    public void GetEventById_NonExistentEvent_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var @event = _repository.GetEventById(id);

        // Assert
        Assert.Null(@event);
    }
}