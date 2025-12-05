using DigitalPrizes.Api.Models.Dtos;
using FluentValidation;

namespace DigitalPrizes.Api.Validators;

/// <summary>
/// Validator for UpdatePrizeDto.
/// </summary>
public class UpdatePrizeDtoValidator : AbstractValidator<UpdatePrizeDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePrizeDtoValidator"/> class.
    /// </summary>
    public UpdatePrizeDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Name must be 100 characters or less")
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must be 500 characters or less")
            .When(x => x.Description is not null);

        RuleFor(x => x.MonetaryValue)
            .GreaterThan(0)
            .WithMessage("Monetary value must be greater than zero")
            .When(x => x.MonetaryValue.HasValue);
    }
}
