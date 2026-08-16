using FluentValidation;

namespace ZakirPro.Features.Teachers.Assignments.GradeSubmission;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.AssignmentId).NotEmpty().WithMessage("AssignmentId is required.");
        RuleFor(x => x.SubmissionId).NotEmpty().WithMessage("SubmissionId is required.");
        RuleFor(x => x.Score).GreaterThanOrEqualTo(0).WithMessage("Score cannot be negative.");
        RuleFor(x => x.Feedback).MaximumLength(2000).WithMessage("Feedback cannot exceed 2000 characters.");
    }
}
