using FluentValidation;
using Xixihihi.Application.Features.Products.Commands;

namespace Xixihihi.Application.Validators.Products;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Product title is required.")
            .MaximumLength(255).WithMessage("Product title must not exceed 255 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Product description is required.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0.");

        RuleFor(x => x.Condition)
            .IsInEnum().WithMessage("Invalid product condition.");

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage("Invalid transaction type.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(x => x.ProvinceId)
            .NotEmpty().WithMessage("Province ID is required.");

        RuleFor(x => x.WardId)
            .NotEmpty().WithMessage("Ward ID is required.");
    }
}
