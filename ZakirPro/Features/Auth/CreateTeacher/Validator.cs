using FluentValidation;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.CreateTeacher;

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

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.Gender)
            .Must(g => Enum.IsDefined(typeof(Gender), g))
            .WithMessage("Gender must be a valid value (Male or Female).");

        RuleFor(x => x.Assignments)
            .NotEmpty().WithMessage("At least one subject/stage assignment is required.");

        RuleForEach(x => x.Assignments).ChildRules(assignment =>
        {
            assignment.RuleFor(a => a.SubjectId)
                .NotEmpty().WithMessage("SubjectId must not be empty.");

            assignment.RuleFor(a => a.StageId)
                .NotEmpty().WithMessage("StageId must not be empty.");
        });
    }
}
