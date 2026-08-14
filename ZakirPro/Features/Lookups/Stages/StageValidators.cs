using FluentValidation;

namespace ZakirPro.Features.Lookups.Stages;

public class CreateStageValidator : AbstractValidator<CreateStageCommand>
{
    public CreateStageValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Stage name is required.")
            .MaximumLength(200).WithMessage("Stage name must not exceed 200 characters.");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be zero or greater.");
    }
}

public class UpdateStageValidator : AbstractValidator<UpdateStageCommand>
{
    public UpdateStageValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Stage ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Stage name is required.")
            .MaximumLength(200).WithMessage("Stage name must not exceed 200 characters.");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be zero or greater.");
    }
}
