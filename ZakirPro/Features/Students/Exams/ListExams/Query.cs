using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Students.Exams.ListExams;

public record Query(Guid StudentId, string? Tab)
    : IRequest<EndpointResponse<List<StudentExamListDto>>>;

public record StudentExamListDto(
    Guid     ExamId,
    string   Title,
    string   SubjectName,
    string   StageName,
    string   TeacherFullName,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    int      DurationMinutes,
    string   StudentStatus,  // NotStarted | InProgress | Submitted | PendingGrading | Graded
    Guid?    AttemptId);
