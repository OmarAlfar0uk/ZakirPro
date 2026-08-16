using FluentValidation;

namespace ZakirPro.Features.Students.Assignments.SubmitAssignment;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("StudentId is required.");

        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("AssignmentId is required.");

        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required.")
            .Must(f => f != null && f.Length > 0).WithMessage("Uploaded file cannot be empty.");
    }
}
