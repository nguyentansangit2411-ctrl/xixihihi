using FluentValidation;
using Xixihihi.Application.Features.Products.Commands;

namespace Xixihihi.Application.Validators.Products;

public class UploadProductImageCommandValidator : AbstractValidator<UploadProductImageCommand>
{
    public UploadProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller ID is required.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(type => type.StartsWith("image/")).WithMessage("Only image files are allowed.");
            
        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File content is required.");
    }
}
