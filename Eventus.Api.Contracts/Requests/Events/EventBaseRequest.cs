using System.ComponentModel.DataAnnotations;

namespace Eventus.Api.Contracts.Requests.Events;

public class EventBaseRequest : IValidatableObject
{
    [Required(ErrorMessage = "Title is required")]
    public string Title { get; init; } = null!;

    public string? Description { get; init; }

    public DateTime StartAt { get; init; }

    public DateTime EndAt { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartAt == default)
        {
            yield return new ValidationResult("Start date must be filled.", [nameof(StartAt)]);
        }

        if (EndAt == default)
        {
            yield return new ValidationResult("End date must be filled.", [nameof(EndAt)]);
        }

        if (StartAt != default && EndAt != default && StartAt > EndAt)
        {
            yield return new ValidationResult("Start date must not be later than end date.",
                [nameof(StartAt), nameof(EndAt)]);
        }
    }
}