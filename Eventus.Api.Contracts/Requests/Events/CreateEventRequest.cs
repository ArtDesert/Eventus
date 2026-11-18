using System.ComponentModel.DataAnnotations;

namespace Eventus.Api.Contracts.Requests.Events;

public class CreateEventRequest : EventBaseRequest
{
    public int TotalSeats { get; init; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in base.Validate(validationContext))
        {
            yield return result;
        }

        if (TotalSeats <= 0)
        {
            yield return new ValidationResult(
                "Total count of seats must be greater than zero.", [nameof(TotalSeats)]);
        }
    }
}