using FluentValidation;

namespace ZakirPro.Features.Auth.Refresh;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
