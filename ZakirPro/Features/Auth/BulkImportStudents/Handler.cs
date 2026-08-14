using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.BulkImportStudents;

public class Handler : IRequestHandler<Command, EndpointResponse<BulkImportResult>>
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

    public async Task<EndpointResponse<BulkImportResult>> Handle(Command request, CancellationToken cancellationToken)
    {
        // 1. Validate file extension
        if (request.File is null || request.File.Length == 0)
            return EndpointResponse<BulkImportResult>.ErrorResponse(
                "No file was provided or the file is empty.");

        var fileName = request.File.FileName ?? string.Empty;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".xls")
            return EndpointResponse<BulkImportResult>.ErrorResponse(
                "Invalid file format. Only .xlsx and .xls files are accepted.");

        // 2. Validate TeacherSubjectStage exists
        var tssRepo = _uow.GetRepository<TeacherSubjectStage>();
        var tss = await tssRepo.FirstOrDefaultAsync(t =>
            t.TeacherId == request.TeacherId &&
            t.SubjectId == request.SubjectId &&
            t.StageId == request.StageId);

        if (tss is null)
            return EndpointResponse<BulkImportResult>.NotFoundResponse(
                "No matching teacher/subject/stage assignment found.");

        // 3. Parse Excel
        var userRepo = _uow.GetRepository<User>();
        var studentLinkRepo = _uow.GetRepository<StudentTeacherSubjectStage>();

        var failedRows = new List<FailedRow>();
        var validStudents = new List<Student>();
        var validLinks = new List<StudentTeacherSubjectStage>();
        var seenEmailsInBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        int totalRows;

        using (var wb = new XLWorkbook(request.File.OpenReadStream()))
        {
            var ws = wb.Worksheet(1);
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            totalRows = Math.Max(0, lastRow - 1); // Row 1 is header

            for (var rowNum = 2; rowNum <= lastRow; rowNum++)
            {
                var row = ws.Row(rowNum);
                var fullName = row.Cell(1).GetString()?.Trim() ?? string.Empty;
                var email = row.Cell(2).GetString()?.Trim() ?? string.Empty;

                var rowErrors = new List<string>();

                // Validate FullName
                if (string.IsNullOrWhiteSpace(fullName))
                    rowErrors.Add("Full name is required.");
                else if (fullName.Length > 200)
                    rowErrors.Add("Full name must not exceed 200 characters.");

                // Validate Email format
                if (string.IsNullOrWhiteSpace(email))
                {
                    rowErrors.Add("Email is required.");
                }
                else if (!IsValidEmail(email))
                {
                    rowErrors.Add("Email format is invalid.");
                }
                else
                {
                    // Check duplicate within batch
                    if (!seenEmailsInBatch.Add(email))
                    {
                        rowErrors.Add("Duplicate email found in this import file.");
                    }
                    else
                    {
                        // Check duplicate in DB
                        var existsInDb = await userRepo.ExistsAsync(u => u.Email == email);
                        if (existsInDb)
                            rowErrors.Add("A user with this email already exists.");
                    }
                }

                if (rowErrors.Count > 0)
                {
                    failedRows.Add(new FailedRow(rowNum, rowErrors));
                    continue;
                }

                // Build Student entity
                var activationCode = GenerateActivationCode();
                var student = new Student
                {
                    Id = Guid.NewGuid(),
                    FullName = fullName,
                    Email = email,
                    Role = UserRole.Student,
                    IsActive = true,
                    IsDeleted = false,
                    IsActivated = false,
                    ActivationCode = activationCode,
                    ActivationCodeExpiry = DateTime.UtcNow.AddHours(24),
                    CreatedAt = DateTime.UtcNow
                };

                var hasher = new PasswordHasher<User>();
                student.PasswordHash = hasher.HashPassword(student, Guid.NewGuid().ToString());

                var link = new StudentTeacherSubjectStage
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    TeacherSubjectStageId = tss.Id
                };

                validStudents.Add(student);
                validLinks.Add(link);
            }
        }

        // 5. Bulk save
        if (validStudents.Count > 0)
        {
            await userRepo.AddRangeAsync(validStudents.Cast<User>().ToList());
            await studentLinkRepo.AddRangeAsync(validLinks);
            await _uow.SaveChangesAsync();
        }

        // 7. Send activation emails fire-and-forget
        var emailService = _emailService;
        foreach (var s in validStudents)
        {
            var capturedStudent = s;
            _ = Task.Run(async () =>
            {
                try
                {
                    await emailService.SendActivationCodeAsync(
                        capturedStudent.Email,
                        capturedStudent.FullName,
                        capturedStudent.ActivationCode!);
                }
                catch
                {
                    // Swallow — email failures must not surface
                }
            });
        }

        // Audit log
        _ = _auditLogger.LogAsync(
            action: "BulkImportStudents",
            userId: _currentUser.UserId,
            targetId: tss.Id,
            description: $"Bulk imported {validStudents.Count} student(s). {failedRows.Count} row(s) failed.",
            ipAddress: _currentUser.IpAddress);

        // 8. Return result
        return EndpointResponse<BulkImportResult>.SuccessResponse(
            new BulkImportResult(
                TotalRows: totalRows,
                SuccessCount: validStudents.Count,
                FailedCount: failedRows.Count,
                FailedRows: failedRows),
            $"Bulk import complete. {validStudents.Count} student(s) created, {failedRows.Count} row(s) failed.");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch
        {
            return false;
        }
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
