using FluentValidation;

namespace ZakirPro.Features.Teachers.Exams.GradeEssayAnswer;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.PointsAwarded)
            .GreaterThanOrEqualTo(0).WithMessage("PointsAwarded must be 0 or more.");
    }
}
