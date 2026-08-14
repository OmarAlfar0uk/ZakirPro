using FluentValidation;

namespace ZakirPro.Features.Lookups.Subjects;

public class CreateSubjectValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Subject name is required.")
            .MaximumLength(200).WithMessage("Subject name must not exceed 200 characters.");
    }
}

public class UpdateSubjectValidator : AbstractValidator<UpdateSubjectCommand>
{
    public UpdateSubjectValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Subject ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Subject name is required.")
            .MaximumLength(200).WithMessage("Subject name must not exceed 200 characters.");
    }
}
