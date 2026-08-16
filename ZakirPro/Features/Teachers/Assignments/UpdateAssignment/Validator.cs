using FluentValidation;

namespace ZakirPro.Features.Teachers.Assignments.UpdateAssignment;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("AssignmentId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(300).WithMessage("Title cannot exceed 300 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description cannot exceed 4000 characters.");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required.");

        When(x => x.MaxScore.HasValue, () =>
        {
            RuleFor(x => x.MaxScore!.Value)
                .GreaterThan(0).WithMessage("Max score must be greater than 0.")
                .LessThanOrEqualTo(1000).WithMessage("Max score cannot exceed 1000.");
        });
    }
}
