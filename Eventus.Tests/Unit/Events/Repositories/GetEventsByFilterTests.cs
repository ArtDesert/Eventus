using Eventus.AppServices.Data.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;
using Eventus.AppServices.Repositories.Events;
using Eventus.Tests.Unit.Events.Repositories.Cases;
using Eventus.Tests.Unit.Events.Repositories.Data;
using FluentAssertions;

namespace Eventus.Tests.Unit.Events.Repositories;

public class GetEventsByFilterTests
{
    private readonly IEventsRepository _repository = new EventsInMemoryRepository(TestEventsData.All);

    [Theory]
    [MemberData(nameof(GetEventsByFilterTestCases.FilterByTitleTestCases),
        MemberType = typeof(GetEventsByFilterTestCases))]
    [MemberData(nameof(GetEventsByFilterTestCases.FilterByDatesTestCases),
        MemberType = typeof(GetEventsByFilterTestCases))]
    [MemberData(nameof(GetEventsByFilterTestCases.PaginatedEventsTestCases),
        MemberType = typeof(GetEventsByFilterTestCases))]
    [MemberData(nameof(GetEventsByFilterTestCases.CombinedFilterTestCases),
        MemberType = typeof(GetEventsByFilterTestCases))]
    public void GetEventsByFilter_ChecksFilteredEventsWithCorrectOrder(
        string _,
        GetEventsByFilterRequest request,
        PaginatedEventsResult expected)
    {
        // Arrange
        // Act
        var actual = _repository.GetEventsByFilter(request);

        // Assert
        actual.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }
}