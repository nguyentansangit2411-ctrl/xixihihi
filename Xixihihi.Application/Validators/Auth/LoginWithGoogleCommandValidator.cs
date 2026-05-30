using FluentValidation;
using Xixihihi.Application.Features.Auth.Commands;

namespace Xixihihi.Application.Validators.Auth;

public class LoginWithGoogleCommandValidator : AbstractValidator<LoginWithGoogleCommand>
{
    public LoginWithGoogleCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Google token is required.");
    }
}
