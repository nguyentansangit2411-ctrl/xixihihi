using FluentValidation;
using Xixihihi.Application.Features.Users.Commands;

namespace Xixihihi.Application.Validators.Users;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.");

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500).WithMessage("Avatar URL must not exceed 500 characters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");

        RuleFor(x => x.ZaloLink)
            .MaximumLength(255).WithMessage("Zalo link must not exceed 255 characters.");

        RuleFor(x => x.FacebookLink)
            .MaximumLength(255).WithMessage("Facebook link must not exceed 255 characters.");
    }
}
