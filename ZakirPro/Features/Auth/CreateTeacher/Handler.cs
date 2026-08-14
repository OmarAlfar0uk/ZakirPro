using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.CreateTeacher;

public class Handler : IRequestHandler<Command, EndpointResponse<CreateTeacherResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _auditLogger;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser, IAuditLogger auditLogger)
    {
        _uow = uow;
        _currentUser = currentUser;
        _auditLogger = auditLogger;
    }

    public async Task<EndpointResponse<CreateTeacherResponse>> Handle(Command request, CancellationToken cancellationToken)
    {
        var userRepo = _uow.GetRepository<User>();

        // 1. Check email uniqueness
        var emailExists = await userRepo.ExistsAsync(u => u.Email == request.Email);
        if (emailExists)
            return EndpointResponse<CreateTeacherResponse>.ErrorResponse("A user with this email already exists.");

        var subjectRepo = _uow.GetRepository<Subject>();
        var stageRepo = _uow.GetRepository<Stage>();
        var tssRepo = _uow.GetRepository<TeacherSubjectStage>();

        // 2. Validate each assignment
        var validationErrors = new List<string>();
        for (var i = 0; i < request.Assignments.Count; i++)
        {
            var assignment = request.Assignments[i];

            var subjectExists = await subjectRepo.ExistsAsync(s => s.Id == assignment.SubjectId);
            if (!subjectExists)
                validationErrors.Add($"Assignment {i + 1}: Subject with ID '{assignment.SubjectId}' does not exist.");

            var stageExists = await stageRepo.ExistsAsync(s => s.Id == assignment.StageId);
            if (!stageExists)
                validationErrors.Add($"Assignment {i + 1}: Stage with ID '{assignment.StageId}' does not exist.");
        }

        if (validationErrors.Count > 0)
            return EndpointResponse<CreateTeacherResponse>.ValidationErrorResponse(validationErrors);

        // Temporary ID to check for duplicates against (teacher doesn't exist yet, so we just check duplicates in request itself)
        // We will create a teacher ID first
        var teacherId = Guid.NewGuid();

        // Check for duplicate assignments within the request itself
        var assignmentSet = new HashSet<(Guid, Guid)>();
        var duplicateErrors = new List<string>();
        for (var i = 0; i < request.Assignments.Count; i++)
        {
            var assignment = request.Assignments[i];
            var key = (assignment.SubjectId, assignment.StageId);
            if (!assignmentSet.Add(key))
                duplicateErrors.Add($"Assignment {i + 1}: Duplicate Subject+Stage combination in request.");
        }

        if (duplicateErrors.Count > 0)
            return EndpointResponse<CreateTeacherResponse>.ValidationErrorResponse(duplicateErrors);

        // 3. Create Teacher entity
        var teacher = new Teacher
        {
            Id = teacherId,
            FullName = request.FullName,
            Email = request.Email,
            Role = UserRole.Teacher,
            Gender = request.Gender,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var hasher = new PasswordHasher<User>();
        teacher.PasswordHash = hasher.HashPassword(teacher, request.Password);

        // 4. Create TeacherSubjectStage records
        var teacherSubjectStages = request.Assignments.Select(a => new TeacherSubjectStage
        {
            Id = Guid.NewGuid(),
            TeacherId = teacherId,
            SubjectId = a.SubjectId,
            StageId = a.StageId
        }).ToList();

        // 5. Save
        await userRepo.AddAsync(teacher);
        await tssRepo.AddRangeAsync(teacherSubjectStages);
        await _uow.SaveChangesAsync();

        // 6. Audit log (fire-and-forget)
        _ = _auditLogger.LogAsync(
            action: "CreateTeacher",
            userId: _currentUser.UserId,
            targetId: teacher.Id,
            description: $"Created teacher account for {teacher.Email} with {teacherSubjectStages.Count} assignment(s).",
            ipAddress: _currentUser.IpAddress);

        // 7. Return response
        return EndpointResponse<CreateTeacherResponse>.SuccessResponse(
            new CreateTeacherResponse(
                teacher.Id,
                teacher.FullName,
                teacher.Email,
                teacher.Gender.ToString(),
                request.Assignments),
            "Teacher account created successfully.");
    }
}
