namespace Eventus.Api.Contracts.Dto.Events;

public class PaginatedEventsResultDto
{
    public required int TotalCount { get; init; }

    public required EventDto[] Items { get; init; }

    public required int Page { get; init; }

    public required int ItemsCount { get; init; }
}