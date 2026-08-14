using FluentValidation;

namespace ZakirPro.Features.Auth.ActivateStudent;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.ActivationCode)
            .NotEmpty().WithMessage("Activation code is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}
