using FluentValidation;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Exams.AddQuestion;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Question text is required.")
            .MaximumLength(2000).WithMessage("Question text must not exceed 2000 characters.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Question type is required.")
            .Must(t => Enum.TryParse<QuestionType>(t, ignoreCase: true, out _))
            .WithMessage("Invalid question type. Valid values: SingleChoice, TrueFalse, Essay.");

        RuleFor(x => x.Points)
            .GreaterThanOrEqualTo(1).WithMessage("Points must be at least 1.");

        // SingleChoice: exactly 4 choices, exactly 1 correct
        When(x => x.Type.Equals("SingleChoice", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Choices)
                .Must(c => c.Count == 4)
                .WithMessage("SingleChoice questions must have exactly 4 choices.");
            RuleFor(x => x.Choices)
                .Must(c => c.Count(ch => ch.IsCorrect) == 1)
                .WithMessage("SingleChoice questions must have exactly one correct choice.");
        });

        // TrueFalse: exactly 2 choices, exactly 1 correct
        When(x => x.Type.Equals("TrueFalse", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Choices)
                .Must(c => c.Count == 2)
                .WithMessage("TrueFalse questions must have exactly 2 choices.");
            RuleFor(x => x.Choices)
                .Must(c => c.Count(ch => ch.IsCorrect) == 1)
                .WithMessage("TrueFalse questions must have exactly one correct choice.");
        });

        // Essay: no choices
        When(x => x.Type.Equals("Essay", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Choices)
                .Must(c => c.Count == 0)
                .WithMessage("Essay questions must have no choices.");
        });

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
