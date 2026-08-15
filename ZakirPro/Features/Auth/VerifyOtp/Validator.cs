using FluentValidation;

namespace ZakirPro.Features.Auth.VerifyOtp;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("OTP is required.")
            .Length(6).WithMessage("OTP must be exactly 6 digits.")
            .Matches("^[0-9]{6}$").WithMessage("OTP must consist of 6 numeric digits.");
    }
}
