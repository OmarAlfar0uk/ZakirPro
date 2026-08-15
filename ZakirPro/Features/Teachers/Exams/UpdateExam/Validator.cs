using FluentValidation;

namespace ZakirPro.Features.Teachers.Exams.UpdateExam;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .When(x => x.Title is not null);

        // If ScheduledEnd is being patched it must still be in the future
        RuleFor(x => x.ScheduledEnd)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("ScheduledEnd must be a future date.")
            .When(x => x.ScheduledEnd.HasValue);
    }
}
