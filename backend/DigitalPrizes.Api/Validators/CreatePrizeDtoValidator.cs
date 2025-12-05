using DigitalPrizes.Api.Models.Dtos;
using FluentValidation;

namespace DigitalPrizes.Api.Validators;

/// <summary>
/// Validator for CreatePrizeDto.
/// </summary>
public class CreatePrizeDtoValidator : AbstractValidator<CreatePrizeDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePrizeDtoValidator"/> class.
    /// </summary>
    public CreatePrizeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name must be 100 characters or less");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must be 500 characters or less");

        RuleFor(x => x.MonetaryValue)
            .GreaterThan(0)
            .WithMessage("Monetary value must be greater than zero")
            .When(x => x.MonetaryValue.HasValue);
    }
}
