using FluentValidation;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Exams.UpdateQuestion;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Question text is required.")
            .MaximumLength(2000).WithMessage("Question text must not exceed 2000 characters.");

        RuleFor(x => x.Points)
            .GreaterThanOrEqualTo(1).WithMessage("Points must be at least 1.");

        RuleForEach(x => x.Choices).ChildRules(choice =>
        {
            choice.RuleFor(c => c.Text)
                .NotEmpty().WithMessage("Choice text is required.")
                .MaximumLength(500).WithMessage("Choice text must not exceed 500 characters.");
            choice.RuleFor(c => c.OrderIndex)
                .GreaterThanOrEqualTo(1).WithMessage("Choice OrderIndex must be at least 1.");
        });
    }
}
