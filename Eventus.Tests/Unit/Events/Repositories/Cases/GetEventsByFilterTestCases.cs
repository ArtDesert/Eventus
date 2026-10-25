using Eventus.AppServices.Data.Dto.Events;
using Eventus.AppServices.Data.Requests.Events;
using Eventus.Tests.Unit.Events.Repositories.Data;

namespace Eventus.Tests.Unit.Events.Repositories.Cases;

public class GetEventsByFilterTestCases
{
    private static Event[] AllOrderedEvents =>
    [
        TestEventsData.DifficultEvent,
        TestEventsData.Event23,
        TestEventsData.SimpleConstEvent,
        TestEventsData.HappyNewYear,
        TestEventsData.TheInternational,
        TestEventsData.BlastSlam,
        TestEventsData.PyrokinesisConcert
    ];

    public static TheoryData<string, GetEventsByFilterRequest, PaginatedEventsResult> FilterByTitleTestCases => new()
    {
        {
            "FilterByTitle_TitleMatchesAtStartOrEnd_ReturnsFilteredEvents",
            new GetEventsByFilterRequest(
                "eVeNt",
                null,
                null,
                1,
                10),
            new PaginatedEventsResult(3, [
                TestEventsData.DifficultEvent,
                TestEventsData.Event23,
                TestEventsData.SimpleConstEvent
            ])
        },
        {
            "FilterByTitle_TitleIsNumber_ReturnsFilteredEvents",
            new GetEventsByFilterRequest(
                "2026",
                null,
                null,
                1,
                10),
            new PaginatedEventsResult(2, [
                TestEventsData.HappyNewYear,
                TestEventsData.TheInternational
            ])
        },
        {
            "FilterByTitle_TitleMatchesInMiddle_ReturnsFilteredEvents",
            new GetEventsByFilterRequest(
                "CO",
                null,
                null,
                1,
                10),
            new PaginatedEventsResult(2, [
                TestEventsData.SimpleConstEvent,
                TestEventsData.PyrokinesisConcert
            ])
        },
        {
            "FilterByTitle_TitleIsNull_ReturnsAllEvents",
            new GetEventsByFilterRequest(
                null,
                null,
                null,
                1,
                10),
            new PaginatedEventsResult(7, AllOrderedEvents)
        },
        {
            "FilterByTitle_TitleIsWhiteSpace_ReturnsAllEvents",
            new GetEventsByFilterRequest(
                "   ",
                null,
                null,
                1,
                10),
            new PaginatedEventsResult(7, AllOrderedEvents)
        },
    };

    public static TheoryData<string, GetEventsByFilterRequest, PaginatedEventsResult> FilterByDatesTestCases => new()
    {
        {
            "FilterByDates_OnlyFromDate_ReturnsFilteredEvents",
            new GetEventsByFilterRequest(
                null,
                new DateTime(2026, 8, 8),
                null,
                1,
                10),
            new PaginatedEventsResult(3, [
                TestEventsData.TheInternational,
                TestEventsData.BlastSlam,
                TestEventsData.PyrokinesisConcert
            ])
        },
        {
            "FilterByDates_OnlyToDate_ReturnsFilteredEvents",
            new GetEventsByFilterRequest(
                null,
                null,
                new DateTime(2025, 1, 1),
                1,
                10),
            new PaginatedEventsResult(2, [
                TestEventsData.DifficultEvent,
                TestEventsData.Event23
            ])
        },
        {
            "FilterByDates_FromAndToDates_ReturnsFilteredEvents",
            new GetEventsByFilterRequest(
                null,
                new DateTime(2024, 1, 1),
                new DateTime(2026, 11, 11),
                1,
                10),
            new PaginatedEventsResult(4, [
                TestEventsData.SimpleConstEvent,
                TestEventsData.HappyNewYear,
                TestEventsData.TheInternational,
                TestEventsData.BlastSlam
            ])
        },
        {
            "FilterByDates_FromAndToDates_ReturnsEmptyResult",
            new GetEventsByFilterRequest(
                null,
                new DateTime(2030, 1, 1),
                new DateTime(2031, 1, 1),
                1,
                10),
            new PaginatedEventsResult(0, [])
        },
        {
            "FilterByDates_DatesAreTheSame_ReturnsEmptyResult",
            new GetEventsByFilterRequest(
                null,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 1),
                1,
                10),
            new PaginatedEventsResult(0, [])
        },
        {
            "FilterByDates_DatesAreTheSame_ReturnsFilteredEvents",
            new GetEventsByFilterRequest(
                null,
                new DateTime(2026, 11, 26),
                new DateTime(2026, 11, 26),
                1,
                10),
            new PaginatedEventsResult(1, [TestEventsData.PyrokinesisConcert])
        },
    };

    public static TheoryData<string, GetEventsByFilterRequest, PaginatedEventsResult> PaginatedEventsTestCases => new()
    {
        {
            "PaginatedEvents_PageIs1AndPageSizeIs1_ReturnsCompletePage",
            new GetEventsByFilterRequest(
                null,
                null,
                null,
                1,
                1),
            new PaginatedEventsResult(7, [TestEventsData.DifficultEvent])
        },
        {
            "PaginatedEvents_PageIs3AndPageSizeIs2_ReturnsCompletePage",
            new GetEventsByFilterRequest(
                null,
                null,
                null,
                3,
                2),
            new PaginatedEventsResult(7, [
                TestEventsData.TheInternational,
                TestEventsData.BlastSlam
            ])
        },
        {
            "PaginatedEvents_PageIs4AndPageSizeIs2_ReturnsIncompletePage",
            new GetEventsByFilterRequest(
                null,
                null,
                null,
                4,
                2),
            new PaginatedEventsResult(7, [TestEventsData.PyrokinesisConcert])
        },
        {
            "PaginatedEvents_ThereAreNoEventsOnPage_ReturnsEmptyResult",
            new GetEventsByFilterRequest(
                null,
                null,
                null,
                100,
                100),
            new PaginatedEventsResult(7, [])
        },
    };

    public static TheoryData<string, GetEventsByFilterRequest, PaginatedEventsResult> CombinedFilterTestCases =>
        new()
        {
            {
                "CombinedFilter_ReturnsCompletePage",
                new GetEventsByFilterRequest(
                    "eve",
                    new DateTime(2000, 1, 1),
                    new DateTime(2024, 1, 1),
                    2,
                    1),
                new PaginatedEventsResult(2, [TestEventsData.Event23])
            },
            {
                "CombinedFilter_ReturnsIncompletePage",
                new GetEventsByFilterRequest(
                    "           ",
                    new DateTime(2012, 1, 1),
                    new DateTime(2026, 11, 11),
                    2,
                    3),
                new PaginatedEventsResult(5, [
                    TestEventsData.TheInternational,
                    TestEventsData.BlastSlam
                ])
            },
            {
                "CombinedFilter_ReturnsEmptyResult",
                new GetEventsByFilterRequest(
                    "abacaba",
                    new DateTime(1990, 1, 1),
                    new DateTime(1995, 1, 1),
                    5,
                    15),
                new PaginatedEventsResult(0, [])
            },
        };
}