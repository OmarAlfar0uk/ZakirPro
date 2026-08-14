using FluentValidation;

namespace ZakirPro.Features.Auth.UpdateProfile;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");
    }
}
