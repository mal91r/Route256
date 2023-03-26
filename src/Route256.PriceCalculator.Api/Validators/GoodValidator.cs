using FluentValidation;
using Route256.PriceCalculator.Domain.Models.PriceCalculator;

namespace Route256.PriceCalculator.Api.Validators;

internal sealed class GoodValidator : AbstractValidator<IReadOnlyCollection<GoodModel>>
{
    public GoodValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Any());
        RuleForEach(x => x)
            .SetValidator(new GoodModelValidator());
    }
}