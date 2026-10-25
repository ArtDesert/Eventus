using Eventus.Api.Contracts.Dto.Events;
using Eventus.AppServices.Data.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;
using Eventus.AppServices.Exceptions;
using Eventus.AppServices.Repositories.Events;
using Eventus.AppServices.Services.Events;
using FluentAssertions;
using Moq;

namespace Eventus.Tests.Unit.Events.Services;

public class EventsServiceTests
{
    private readonly Mock<IEventsRepository> _repositoryMock;
    private readonly IEventsService _service;

    public EventsServiceTests()
    {
        _repositoryMock = new Mock<IEventsRepository>();
        _service = new EventsService(_repositoryMock.Object);
    }

    #region PositiveCases

    [Fact]
    public void CreateEvent_CorrectRequest_ReturnsCreatedEventId()
    {
        // Arrange
        var expected = Guid.NewGuid();
        var request = new CreateEventRequest(
            "Test",
            null,
            new DateTime(2000, 1, 1),
            new DateTime(2005, 1, 1));
        _repositoryMock.Setup(mock => mock.CreateEvent(request)).Returns(expected);

        // Act
        var actual = _service.CreateEvent(request);

        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void GetEventsByFilter_RepositoryReturnsEvents_MapsToPaginatedResultDto()
    {
        // Arrange
        var request = new GetEventsByFilterRequest(null, null, null, 5, 10);
        var id = Guid.NewGuid();
        var expected = new PaginatedEventsResultDto
        {
            TotalCount = 41,
            Items =
            [
                new EventDto
                {
                    Id = id,
                    Title = "TITLE",
                    Description = "DESCRIPTION",
                    StartAt = new DateTime(2000, 1, 1, 9, 0, 0),
                    EndAt = new DateTime(2000, 1, 1, 18, 0, 0),
                }
            ],
            Page = 5,
            ItemsCount = 1
        };
        _repositoryMock.Setup(mock => mock.GetEventsByFilter(request))
            .Returns(new PaginatedEventsResult(41, [
                new Event(
                    id,
                    "TITLE",
                    "DESCRIPTION",
                    new DateTime(2000, 1, 1, 9, 0, 0),
                    new DateTime(2000, 1, 1, 18, 0, 0))
            ]));

        // Act
        var actual = _service.GetEventsByFilter(request);

        // Assert
        Assert.NotNull(actual);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetEventById_ExistingEvent_ReturnsEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expected = new Event(
            id,
            "Test",
            null,
            new DateTime(2000, 1, 1),
            new DateTime(2005, 1, 1));
        _repositoryMock.Setup(mock => mock.GetEventById(id)).Returns(expected);

        // Act
        var actual = _service.GetEventById(id);

        // Assert
        Assert.NotNull(actual);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void UpdateEvent_ExistingEvent_DelegatesToRepository()
    {
        // Arrange
        var request = new UpdateEventRequest(
            Guid.NewGuid(),
            "Test",
            null,
            new DateTime(2000, 1, 1),
            new DateTime(2005, 1, 1));
        _repositoryMock.Setup(mock => mock.UpdateEvent(request)).Returns(true);

        // Act
        var exception = Record.Exception(() => _service.UpdateEvent(request));

        // Assert
        Assert.Null(exception);
        _repositoryMock.Verify(mock => mock.UpdateEvent(request), Times.Once);
    }

    [Fact]
    public void DeleteEvent_ExistingEvent_DelegatesToRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(mock => mock.DeleteEvent(id)).Returns(true);

        // Act
        var exception = Record.Exception(() => _service.DeleteEvent(id));

        // Assert
        Assert.Null(exception);
        _repositoryMock.Verify(mock => mock.DeleteEvent(id), Times.Once);
    }

    #endregion Positive cases.

    #region NegativeCases

    [Fact]
    public void GetEventById_NonExistentEvent_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        Event? expected = null;
        _repositoryMock.Setup(mock => mock.GetEventById(id)).Returns(expected);

        // Act & Assert
        Assert.Throws<NotFoundException>(() => _service.GetEventById(id));
    }

    [Fact]
    public void UpdateEvent_NonExistentEvent_ThrowsNotFoundException()
    {
        // Arrange
        var request = new UpdateEventRequest(
            Guid.NewGuid(),
            "Test",
            null,
            new DateTime(2000, 1, 1),
            new DateTime(2005, 1, 1));
        _repositoryMock.Setup(mock => mock.UpdateEvent(request)).Returns(false);

        // Act & Assert
        Assert.Throws<NotFoundException>(() => _service.UpdateEvent(request));
        _repositoryMock.Verify(mock => mock.UpdateEvent(request), Times.Once);
    }

    [Fact]
    public void DeleteEvent_NonExistentEvent_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(mock => mock.DeleteEvent(id)).Returns(false);

        // Act & Assert
        Assert.Throws<NotFoundException>(() =>_service.DeleteEvent(id));
        _repositoryMock.Verify(mock => mock.DeleteEvent(id), Times.Once);
    }

    #endregion
}