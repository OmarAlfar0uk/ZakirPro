using FluentValidation;

namespace ZakirPro.Features.Teachers.Lectures.UpdateLecture;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.LectureId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.GoogleDriveLink).NotEmpty().MaximumLength(1000);
    }
}
