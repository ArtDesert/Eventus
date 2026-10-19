using System.ComponentModel.DataAnnotations;

namespace Eventus.WebApi.Data.Requests.Events;

public class EventBaseRequest : IValidatableObject
{
    [Required(ErrorMessage = "Title is required")]
    public required string Title { get; init; }
    
    public string? Description { get; init; }
    
    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartAt { get; init; }
    
    [Required(ErrorMessage = "End date is required")]
    public DateTime EndAt { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartAt > EndAt)
        {
            yield return new ValidationResult("Start date must be before end date.", [nameof(StartAt), nameof(EndAt)]);
        }
    }
}