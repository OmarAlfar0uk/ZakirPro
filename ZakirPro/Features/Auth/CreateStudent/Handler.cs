using MediatR;
using Microsoft.AspNetCore.Identity;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.CreateStudent;

public class Handler : IRequestHandler<Command, EndpointResponse<CreateStudentResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _auditLogger;
    private readonly IEmailService _emailService;

    public Handler(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IAuditLogger auditLogger,
        IEmailService emailService)
    {
        _uow = uow;
        _currentUser = currentUser;
        _auditLogger = auditLogger;
        _emailService = emailService;
    }

    public async Task<EndpointResponse<CreateStudentResponse>> Handle(Command request, CancellationToken cancellationToken)
    {
        var userRepo = _uow.GetRepository<User>();

        // 1. Check email uniqueness
        var emailExists = await userRepo.ExistsAsync(u => u.Email == request.Email);
        if (emailExists)
            return EndpointResponse<CreateStudentResponse>.ErrorResponse("A user with this email already exists.");

        // 2. Validate TeacherSubjectStage exists
        var tssRepo = _uow.GetRepository<TeacherSubjectStage>();
        var tss = await tssRepo.FirstOrDefaultAsync(t =>
            t.TeacherId == request.TeacherId &&
            t.SubjectId == request.SubjectId &&
            t.StageId == request.StageId);

        if (tss is null)
            return EndpointResponse<CreateStudentResponse>.NotFoundResponse(
                "No matching teacher/subject/stage assignment found.");

        // 3. Generate activation code
        var activationCode = GenerateActivationCode();

        // 4. Create Student entity
        var student = new Student
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = false,
            IsActivated = false,
            ActivationCode = activationCode,
            ActivationCodeExpiry = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };

        // Password is not set yet — student activates their own account and sets it
        var hasher = new PasswordHasher<User>();
        student.PasswordHash = hasher.HashPassword(student, Guid.NewGuid().ToString()); // placeholder

        // 5. Create StudentTeacherSubjectStage link
        var link = new StudentTeacherSubjectStage
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            TeacherSubjectStageId = tss.Id
        };

        await userRepo.AddAsync(student);
        await _uow.GetRepository<StudentTeacherSubjectStage>().AddAsync(link);
        await _uow.SaveChangesAsync();

        // 7. Send activation email (fire-and-forget)
        var emailService = _emailService;
        var studentName = student.FullName;
        var studentEmail = student.Email;
        _ = Task.Run(async () =>
        {
            try
            {
                await emailService.SendActivationCodeAsync(studentEmail, studentName, activationCode);
            }
            catch
            {
                // Swallow — email failure must not surface to caller
            }
        });

        // 6. Audit log (fire-and-forget)
        _ = _auditLogger.LogAsync(
            action: "CreateStudent",
            userId: _currentUser.UserId,
            targetId: student.Id,
            description: $"Created student account for {student.Email}.",
            ipAddress: _currentUser.IpAddress);

        // 8. Return response including the code (for admin records)
        return EndpointResponse<CreateStudentResponse>.SuccessResponse(
            new CreateStudentResponse(student.Id, student.FullName, student.Email, activationCode),
            "Student account created successfully.");
    }

    private static string GenerateActivationCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = new byte[8];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }
}
