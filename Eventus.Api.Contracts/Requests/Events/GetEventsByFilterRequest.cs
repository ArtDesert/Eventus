using System.ComponentModel.DataAnnotations;

namespace Eventus.Api.Contracts.Requests.Events;

public class GetEventsByFilterRequest : IValidatableObject
{
    public string? Title { get; init; }
    
    public DateTime? From { get; init; }
    
    public DateTime? To { get; init; }

    [Range(1, 100)]
    public int? Page { get; init; }

    [Range(1, 100)]
    public int? PageSize { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From > To)
        {
            yield return new ValidationResult("\"From\" date must not be later than \"to\" date.", [nameof(From), nameof(To)]);
        }
    }
}