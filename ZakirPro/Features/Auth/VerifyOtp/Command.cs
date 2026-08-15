using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.VerifyOtp;

public record Command(string Email, string Otp) : IRequest<EndpointResponse<VerifyOtpResponse>>;

/// <summary>Returned on successful OTP verification. The client must include ResetToken in the final reset call.</summary>
public record VerifyOtpResponse(string ResetToken, DateTime ExpiresAt);
