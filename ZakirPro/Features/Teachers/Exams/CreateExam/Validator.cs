using FluentValidation;

namespace ZakirPro.Features.Teachers.Exams.CreateExam;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.TeacherSubjectStageId)
            .NotEmpty().WithMessage("TeacherSubjectStageId is required.");

        RuleFor(x => x.ScheduledStart)
            .NotEmpty().WithMessage("ScheduledStart is required.");

        RuleFor(x => x.ScheduledEnd)
            .NotEmpty().WithMessage("ScheduledEnd is required.")
            .GreaterThan(x => x.ScheduledStart)
            .WithMessage("ScheduledEnd must be after ScheduledStart.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("DurationMinutes must be at least 1.")
            .Must((cmd, dur) =>
                dur <= (cmd.ScheduledEnd - cmd.ScheduledStart).TotalMinutes)
            .WithMessage("DurationMinutes cannot exceed the length of the scheduled window.");

        RuleFor(x => x.PassThresholdPercent)
            .InclusiveBetween(0, 100)
            .WithMessage("PassThresholdPercent must be between 0 and 100.");
    }
}
