using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Students.Exams.ListExams;

public class Handler : IRequestHandler<Query, EndpointResponse<List<StudentExamListDto>>>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<List<StudentExamListDto>>> Handle(
        Query request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.StudentId)
            return EndpointResponse<List<StudentExamListDto>>.ForbiddenResponse(
                "You are not authorized to view another student's exams.");

        // Get all TSS IDs the student is enrolled in
        var enrolledTssIds = await _uow.GetRepository<StudentTeacherSubjectStage>()
            .Query()
            .Where(s => s.StudentId == request.StudentId)
            .Select(s => s.TeacherSubjectStageId)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        // Load all published exams for those TSS records
        var allExams = await _uow.GetRepository<Exam>()
            .Query()
            .Include(e => e.TeacherSubjectStage)
                .ThenInclude(tss => tss.Subject)
            .Include(e => e.TeacherSubjectStage)
                .ThenInclude(tss => tss.Stage)
            .Include(e => e.TeacherSubjectStage)
                .ThenInclude(tss => tss.Teacher)
            .Where(e =>
                e.Status == ExamStatus.Published &&
                enrolledTssIds.Contains(e.TeacherSubjectStageId))
            .ToListAsync(cancellationToken);

        // Load student's attempts for these exams
        var examIds = allExams.Select(e => e.Id).ToList();
        var attempts = await _uow.GetRepository<StudentExamAttempt>()
            .Query()
            .Where(a => a.StudentId == request.StudentId && examIds.Contains(a.ExamId))
            .ToListAsync(cancellationToken);

        var attemptByExam = attempts.ToDictionary(a => a.ExamId);

        // Determine student status per exam
        string GetStudentStatus(Exam exam)
        {
            if (attemptByExam.TryGetValue(exam.Id, out var attempt))
                return attempt.AttemptStatus.ToString();
            return "NotStarted";
        }

        // Tab filter
        bool MatchesTab(Exam exam)
        {
            var tab = request.Tab?.ToLowerInvariant();
            return tab switch
            {
                "upcoming"   => exam.ScheduledStart > now,
                "inprogress" => exam.ScheduledStart <= now && exam.ScheduledEnd >= now,
                "complete"   => exam.ScheduledEnd < now ||
                                (attemptByExam.ContainsKey(exam.Id) &&
                                 attemptByExam[exam.Id].AttemptStatus is
                                     AttemptStatus.Submitted or
                                     AttemptStatus.PendingGrading or
                                     AttemptStatus.Graded),
                _            => true   // no tab filter — return all
            };
        }

        var result = allExams
            .Where(MatchesTab)
            .OrderBy(e => e.ScheduledStart)
            .Select(e => new StudentExamListDto(
                e.Id,
                e.Title,
                e.TeacherSubjectStage.Subject.Name,
                e.TeacherSubjectStage.Stage.Name,
                e.TeacherSubjectStage.Teacher.FullName,
                e.ScheduledStart,
                e.ScheduledEnd,
                e.DurationMinutes,
                GetStudentStatus(e),
                attemptByExam.TryGetValue(e.Id, out var att) ? att.Id : null))
            .ToList();

        return EndpointResponse<List<StudentExamListDto>>.SuccessResponse(result, "Exams retrieved successfully.");
    }
}
