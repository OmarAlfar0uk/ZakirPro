using FluentValidation;

namespace ZakirPro.Features.Teachers.Lectures.CreateLecture;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.GoogleDriveLink).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.TeacherSubjectStageId).NotEmpty();
    }
}
