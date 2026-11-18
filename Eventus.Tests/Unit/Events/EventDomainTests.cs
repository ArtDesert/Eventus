using Eventus.AppServices.Data.Domain.Events;
using Eventus.AppServices.Exceptions;
using FluentAssertions;

namespace Eventus.Tests.Unit.Events;

public class EventDomainTests
{
    public static TheoryData<string, Guid, string?, string?, DateTime, DateTime, int, string> IncorrectRequests => new()
    {
        {
            "EmptyId", Guid.Empty, "Title", "Description", TestDataFactory.StartDate,
            TestDataFactory.EndDate, 10, nameof(Event.Id)
        },
        {
            "TitleIsNull", TestDataFactory.EventId, null, "Description", TestDataFactory.StartDate,
            TestDataFactory.EndDate, 10, nameof(Event.Title)
        },
        {
            "TitleIsWhiteSpace", TestDataFactory.EventId, "     ", "Description", TestDataFactory.StartDate,
            TestDataFactory.EndDate, 10, nameof(Event.Title)
        },
        {
            "StartDateIsDefault", TestDataFactory.EventId, "Title", "Description", default,
            TestDataFactory.EndDate, 10, nameof(Event.StartAt)
        },
        {
            "EndDateIsDefault", TestDataFactory.EventId, "Title", "Description", TestDataFactory.StartDate,
            default, 10, nameof(Event.EndAt)
        },
        {
            "StartDateIsMoreThanEndDate", TestDataFactory.EventId, "Title", "Description",
            TestDataFactory.EndDate, TestDataFactory.StartDate, 10, nameof(Event.StartAt)
        },
        {
            "TotalSeatsIsNegative", TestDataFactory.EventId, "Title", "Description",
            TestDataFactory.StartDate, TestDataFactory.EndDate, -10, nameof(Event.TotalSeats)
        },
        {
            "TotalSeatsIsZero", TestDataFactory.EventId, "Title", "Description", TestDataFactory.StartDate,
            TestDataFactory.EndDate, 0, nameof(Event.TotalSeats)
        }
    };

    public static TheoryData<string, string?, DateTime, DateTime, string> IncorrectUpdateRequests => new()
    {
        { "TitleIsNull", null, TestDataFactory.StartDate, TestDataFactory.EndDate, nameof(Event.Title) },
        { "TitleIsWhiteSpace", "     ", TestDataFactory.StartDate, TestDataFactory.EndDate, nameof(Event.Title) },
        { "StartDateIsDefault", "Title", default, TestDataFactory.EndDate, nameof(Event.StartAt) },
        { "EndDateIsDefault", "Title", TestDataFactory.StartDate, default, nameof(Event.EndAt) },
        {
            "StartDateIsMoreThanEndDate", "Title", TestDataFactory.EndDate, TestDataFactory.StartDate,
            nameof(Event.StartAt)
        }
    };

    #region NegativeCases

    [InlineData(0)]
    [InlineData(-1)]
    [Theory]
    public void TryReserveSeats_IncorrectCount_ThrowsException(int count)
    {
        // Arrange
        var sourceEvent = TestDataFactory.Event;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => sourceEvent.TryReserveSeats(out _, count));
    }

    [InlineData(11, 10)]
    [InlineData(100, 10)]
    [Theory]
    public void TryReserveSeats_NotEnoughSeatsToReserve_ReturnsFalseAndNull(int count, int totalSeats)
    {
        // Arrange
        var sourceEvent = TestDataFactory.BuildEvent(totalSeats: totalSeats);
        var oldAvailableSeats = sourceEvent.AvailableSeats;

        // Act
        var isReserved = sourceEvent.TryReserveSeats(out var updatedEvent, count);

        // Assert
        Assert.False(isReserved);
        Assert.Null(updatedEvent);
        Assert.Equal(oldAvailableSeats, sourceEvent.AvailableSeats);
    }

    [InlineData(0)]
    [InlineData(-1)]
    [Theory]
    public void ReleaseSeats_IncorrectCount_ThrowsException(int count)
    {
        // Arrange
        var sourceEvent = TestDataFactory.Event;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => sourceEvent.ReleaseSeats(count));
    }

    [InlineData(3, 4, 10)]
    [InlineData(4, 50, 10)]
    [Theory]
    public void ReleaseSeats_NotEnoughSeatsToRelease_ThrowsException(
        int reserveCount, int releaseCount, int totalSeats)
    {
        // Arrange
        TestDataFactory.BuildEvent(totalSeats: totalSeats).TryReserveSeats(out var sourceEvent, reserveCount);
        Assert.NotNull(sourceEvent);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => sourceEvent.ReleaseSeats(releaseCount));
    }

    [InlineData(1, 10)]
    [InlineData(100, 10)]
    [Theory]
    public void ReleaseSeats_NoReservedSeats_ThrowsException(
        int releaseCount, int totalSeats)
    {
        // Arrange
        var sourceEvent = TestDataFactory.BuildEvent(totalSeats: totalSeats);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => sourceEvent.ReleaseSeats(releaseCount));
    }

    [Theory]
    [MemberData(nameof(IncorrectRequests))]
    public void CreateEvent_IncorrectRequests_ThrowsDomainValidationException(
        string _,
        Guid id,
        string? title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        int totalSeats,
        string field)
    {
        // Arrange
        // Act
        var exception = Assert.Throws<DomainValidationException>(() => new Event(id, title!, description, startAt, endAt, totalSeats));

        // Assert
        Assert.Equal(field, exception.ProblemFieldName);
    }

    [Theory]
    [MemberData(nameof(IncorrectUpdateRequests))]
    public void UpdateEvent_IncorrectRequests_ThrowsDomainValidationException(
        string _,
        string? title,
        DateTime startAt,
        DateTime endAt,
        string field)
    {
        // Arrange
        var sourceEvent = TestDataFactory.BuildEvent(totalSeats: 10);

        // Act
        var exception = Assert.Throws<DomainValidationException>(
            () => sourceEvent.Update(title!, "Description", startAt, endAt));

        // Assert
        Assert.Equal(field, exception.ProblemFieldName);
    }

    #endregion

    #region PositiveCases

    [InlineData(1, 10)]
    [InlineData(10, 10)]
    [Theory]
    public void TryReserveSeats_EnoughSeatsToReserve_ReturnsTrueAndUpdatedEvent(int count, int totalSeats)
    {
        // Arrange
        var sourceEvent = TestDataFactory.BuildEvent(totalSeats: totalSeats);
        var oldAvailableSeats = sourceEvent.AvailableSeats;

        // Act
        var isReserved = sourceEvent.TryReserveSeats(out var updatedEvent, count);

        // Assert
        Assert.True(isReserved);
        Assert.NotNull(updatedEvent);
        updatedEvent.Should().BeEquivalentTo(sourceEvent, o => o.Excluding(x => x.AvailableSeats));
        Assert.Equal(sourceEvent.AvailableSeats - count, updatedEvent.AvailableSeats);
        Assert.Equal(oldAvailableSeats, sourceEvent.AvailableSeats);
    }

    [InlineData(3, 1, 10)]
    [InlineData(5, 5, 10)]
    [Theory]
    public void ReleaseSeats_EnoughSeatsToRelease_ReturnsUpdatedEvent(
        int reserveCount, int releaseCount, int totalSeats)
    {
        // Arrange
        TestDataFactory.BuildEvent(totalSeats: totalSeats).TryReserveSeats(out var sourceEvent, reserveCount);
        Assert.NotNull(sourceEvent);
        var oldAvailableSeats = sourceEvent.AvailableSeats;

        // Act
        var updatedEvent = sourceEvent.ReleaseSeats(releaseCount);

        // Assert
        Assert.NotNull(updatedEvent);
        updatedEvent.Should().BeEquivalentTo(sourceEvent, o => o.Excluding(x => x.AvailableSeats));
        Assert.Equal(sourceEvent.AvailableSeats + releaseCount, updatedEvent.AvailableSeats);
        Assert.Equal(oldAvailableSeats, sourceEvent.AvailableSeats);
    }

    [Fact]
    public void CreateEvent_CorrectRequest_ReturnsCreatedEvent()
    {
        // Arrange
        var eventId = TestDataFactory.EventId;
        const string title = "Title";
        const string description = "Description";
        const int totalSeats = 10;
        var startAt = TestDataFactory.StartDate;
        var endAt = TestDataFactory.EndDate;

        // Act
        var actual = new Event(eventId, title, description, startAt, endAt, totalSeats);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(eventId, actual.Id);
        Assert.Equal(title, actual.Title);
        Assert.Equal(description, actual.Description);
        Assert.Equal(startAt, actual.StartAt);
        Assert.Equal(endAt, actual.EndAt);
        Assert.Equal(totalSeats, actual.TotalSeats);
        Assert.Equal(totalSeats, actual.AvailableSeats);
    }

    [Fact]
    public void UpdateEvent_CorrectRequest_ReturnsUpdatedEvent()
    {
        // Arrange
        var sourceEvent = TestDataFactory.BuildEvent(totalSeats: 10);
        var oldTitle = sourceEvent.Title;
        const string title = "New title";
        const string description = "New description";
        var startAt = TestDataFactory.StartDate.AddDays(1);
        var endAt = TestDataFactory.EndDate.AddDays(1);

        // Act
        var actual = sourceEvent.Update(title, description, startAt, endAt);

        // Assert
        Assert.NotNull(actual);
        Assert.NotSame(sourceEvent, actual);
        Assert.Equal(sourceEvent.Id, actual.Id);
        Assert.Equal(title, actual.Title);
        Assert.Equal(description, actual.Description);
        Assert.Equal(startAt, actual.StartAt);
        Assert.Equal(endAt, actual.EndAt);
        Assert.Equal(sourceEvent.TotalSeats, actual.TotalSeats);
        Assert.Equal(sourceEvent.AvailableSeats, actual.AvailableSeats);
        Assert.Equal(oldTitle, sourceEvent.Title);
    }

    [InlineData(1, 10)]
    [InlineData(10, 10)]
    [Theory]
    public void UpdateEvent_EventWithReservedSeats_PreservesAvailableSeats(int reserveCount, int totalSeats)
    {
        // Arrange
        TestDataFactory.BuildEvent(totalSeats: totalSeats).TryReserveSeats(out var sourceEvent, reserveCount);
        Assert.NotNull(sourceEvent);

        // Act
        var actual = sourceEvent.Update("New title", null, sourceEvent.StartAt, sourceEvent.EndAt);

        // Assert
        Assert.Equal(totalSeats, actual.TotalSeats);
        Assert.Equal(totalSeats - reserveCount, actual.AvailableSeats);
    }

    #endregion
}