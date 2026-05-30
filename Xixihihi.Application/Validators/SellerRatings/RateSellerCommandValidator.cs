using FluentValidation;
using Xixihihi.Application.Features.SellerRatings.Commands;

namespace Xixihihi.Application.Validators.SellerRatings;

public class RateSellerCommandValidator : AbstractValidator<RateSellerCommand>
{
    public RateSellerCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller ID is required.");

        RuleFor(x => x.ReviewerId)
            .NotEqual(x => x.SellerId).WithMessage("You cannot rate yourself.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters.");
    }
}
