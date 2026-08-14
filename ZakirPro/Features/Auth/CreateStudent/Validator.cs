using FluentValidation;

namespace ZakirPro.Features.Auth.CreateStudent;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("TeacherId is required.");

        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("SubjectId is required.");

        RuleFor(x => x.StageId)
            .NotEmpty().WithMessage("StageId is required.");
    }
}
