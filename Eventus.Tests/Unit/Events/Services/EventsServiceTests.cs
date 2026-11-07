using Eventus.Api.Contracts.Dto.Events;
using Eventus.AppServices.Data.Domain.Events;
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
    private readonly CancellationToken _token;

    public EventsServiceTests()
    {
        _repositoryMock = new Mock<IEventsRepository>();
        _service = new EventsService(_repositoryMock.Object);
        _token = CancellationToken.None;
    }

    #region PositiveCases

    [Fact]
    public async Task CreateEvent_CorrectRequest_ReturnsCreatedEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new CreateEventRequest(
            "Test",
            "Description",
            new DateTime(2000, 1, 1),
            new DateTime(2005, 1, 1));
        var expected = new Event(
            id,
            request.Title,
            request.Description,
            request.StartAt,
            request.EndAt);
        _repositoryMock.Setup(mock => mock.CreateEventAsync(request, _token)).ReturnsAsync(expected);

        // Act
        var actual = await _service.CreateEventAsync(request, _token);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetEventsByFilter_RepositoryReturnsEvents_MapsToPaginatedResultDto()
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
        _repositoryMock.Setup(mock => mock.GetEventsByFilterAsync(request, _token))
            .ReturnsAsync(new PaginatedEventsResult(41, [
                new Event(
                    id,
                    "TITLE",
                    "DESCRIPTION",
                    new DateTime(2000, 1, 1, 9, 0, 0),
                    new DateTime(2000, 1, 1, 18, 0, 0))
            ]));

        // Act
        var actual = await _service.GetEventsByFilterAsync(request, _token);

        // Assert
        Assert.NotNull(actual);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetEventById_ExistingEvent_ReturnsEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expected = new Event(
            id,
            "Test",
            null,
            new DateTime(2000, 1, 1),
            new DateTime(2005, 1, 1));
        _repositoryMock.Setup(mock => mock.GetEventByIdAsync(id, _token)).ReturnsAsync(expected);

        // Act
        var actual = await _service.GetEventByIdAsync(id, _token);

        // Assert
        Assert.NotNull(actual);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task UpdateEvent_ExistingEvent_DelegatesToRepository()
    {
        // Arrange
        var request = new UpdateEventRequest(
            Guid.NewGuid(),
            "Test",
            null,
            new DateTime(2000, 1, 1),
            new DateTime(2005, 1, 1));
        _repositoryMock.Setup(mock => mock.UpdateEventAsync(request, _token)).ReturnsAsync(true);

        // Act
        var exception = await Record.ExceptionAsync(() => _service.UpdateEventAsync(request, _token));

        // Assert
        Assert.Null(exception);
        _repositoryMock.Verify(mock => mock.UpdateEventAsync(request, _token), Times.Once);
    }

    [Fact]
    public async Task DeleteEvent_ExistingEvent_DelegatesToRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(mock => mock.DeleteEventAsync(id, _token)).ReturnsAsync(true);

        // Act
        var exception = await Record.ExceptionAsync(() => _service.DeleteEventAsync(id, _token));

        // Assert
        Assert.Null(exception);
        _repositoryMock.Verify(mock => mock.DeleteEventAsync(id, _token), Times.Once);
    }

    #endregion Positive cases.

    #region NegativeCases

    [Fact]
    public async Task GetEventById_NonExistentEvent_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        Event? expected = null;
        _repositoryMock.Setup(mock => mock.GetEventByIdAsync(id, _token)).ReturnsAsync(expected);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetEventByIdAsync(id, _token));
    }

    [Fact]
    public async Task UpdateEvent_NonExistentEvent_ThrowsNotFoundException()
    {
        // Arrange
        var request = new UpdateEventRequest(
            Guid.NewGuid(),
            "Test",
            null,
            new DateTime(2000, 1, 1),
            new DateTime(2005, 1, 1));
        _repositoryMock.Setup(mock => mock.UpdateEventAsync(request, _token)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateEventAsync(request, _token));
        _repositoryMock.Verify(mock => mock.UpdateEventAsync(request, _token), Times.Once);
    }

    [Fact]
    public async Task DeleteEvent_NonExistentEvent_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(mock => mock.DeleteEventAsync(id, _token)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteEventAsync(id, _token));
        _repositoryMock.Verify(mock => mock.DeleteEventAsync(id, _token), Times.Once);
    }

    #endregion
}