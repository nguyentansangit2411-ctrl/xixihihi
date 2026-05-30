using FluentValidation;
using Xixihihi.Application.Features.Products.Commands;

namespace Xixihihi.Application.Validators.Products;

public class UpdateProductStatusCommandValidator : AbstractValidator<UpdateProductStatusCommand>
{
    public UpdateProductStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller ID is required.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid product status.");
    }
}
