using FluentValidation;

namespace ZakirPro.Features.Teachers.SelfService.AddSubjectStage;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.StageId).NotEmpty();
    }
}
