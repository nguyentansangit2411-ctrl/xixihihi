using FluentValidation;
using Xixihihi.Application.Features.Products.Queries;

namespace Xixihihi.Application.Validators.Products;

public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("PageSize must be between 1 and 50.");

        RuleFor(x => x.SortBy)
            .Must(x => x == "price_asc" || x == "price_desc" || x == "newest")
            .When(x => !string.IsNullOrEmpty(x.SortBy))
            .WithMessage("SortBy must be 'price_asc', 'price_desc', or 'newest'.");
    }
}
