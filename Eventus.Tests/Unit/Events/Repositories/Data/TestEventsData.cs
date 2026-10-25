using Eventus.AppServices.Data.Dto.Events;

namespace Eventus.Tests.Unit.Events.Repositories.Data;

public static class TestEventsData
{
    public static readonly Event TheInternational = new(
        Guid.Parse("5eb5fa1b-79d5-42b7-a190-a6532b5e085a"),
        "The International 2026",
        null,
        new DateTime(2026, 08, 13),
        new DateTime(2026, 08, 23));

    public static readonly Event BlastSlam = new(
        Guid.Parse("43c40b54-715e-4151-80e2-17587ed52d08"),
        "BLAST Slam 8",
        "The 8th BLAST Dota 2 Tournament",
        new DateTime(2026, 09, 29),
        new DateTime(2026, 10, 11));


    public static readonly Event PyrokinesisConcert = new(
        Guid.Parse("da3a22e4-655f-4672-aceb-6c5dd183b202"),
        "Pyrokinesis concert",
        "in St. Petersburg",
        new DateTime(2026, 11, 26),
        new DateTime(2026, 11, 26));


    public static readonly Event SimpleConstEvent = new(
        Guid.Parse("607e03a0-843a-49d8-af88-e8086a38b839"),
        "Simple const event",
        null,
        new DateTime(2024, 7, 3),
        new DateTime(2025, 2, 17));


    public static readonly Event HappyNewYear = new(
        Guid.Parse("ac954877-3b09-495b-b267-ebac7745e251"),
        "Happy New Year 2026 holiday",
        null,
        new DateTime(2026, 1, 1),
        new DateTime(2026, 1, 12));

    public static readonly Event Event23 = new(
        Guid.Parse("ed2e2773-460e-46e0-95c1-679d5c3da0f5"),
        "Event 23",
        null,
        new DateTime(2013, 5, 5),
        new DateTime(2023, 6, 6));
    
    public static readonly Event DifficultEvent = new(
        Guid.Parse("ba6f8286-a536-45c0-89a2-d155988c0e93"),
        "Difficult event", null,
        new DateTime(2000, 12, 23),
        new DateTime(2019, 7, 21));
    
    public static IReadOnlyList<Event> All =>
        [TheInternational, BlastSlam, PyrokinesisConcert, SimpleConstEvent, HappyNewYear, Event23, DifficultEvent];
}