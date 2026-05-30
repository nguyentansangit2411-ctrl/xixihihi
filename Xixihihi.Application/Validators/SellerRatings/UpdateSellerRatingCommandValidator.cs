using FluentValidation;
using Xixihihi.Application.Features.SellerRatings.Commands;

namespace Xixihihi.Application.Validators.SellerRatings;

public class UpdateSellerRatingCommandValidator : AbstractValidator<UpdateSellerRatingCommand>
{
    public UpdateSellerRatingCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Rating ID is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters.");
    }
}
