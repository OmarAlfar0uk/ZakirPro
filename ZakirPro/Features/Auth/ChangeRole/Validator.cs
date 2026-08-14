using FluentValidation;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.ChangeRole;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.NewRole)
            .IsInEnum().WithMessage("Invalid role value provided.");
    }
}
