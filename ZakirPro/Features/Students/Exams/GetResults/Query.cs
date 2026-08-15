using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Students.Exams.GetResults;

public record Query(Guid StudentId, Guid AttemptId)
    : IRequest<EndpointResponse<ExamResultDto>>;

public record ExamResultDto(
    Guid     AttemptId,
    string   ExamTitle,
    string   SubjectName,
    string   TeacherFullName,
    DateTime ExamDate,
    string   AttemptStatus,
    string   GradingStatus,    // "Complete" | "PendingEssayGrading"
    int?     FinalScore,
    int      MaxScore,
    decimal? ScorePercent,
    bool?    IsPassed,
    int?     TimeTakenSeconds,
    string?  TeacherFeedback,
    PerformanceSummaryDto PerformanceSummary);

public record PerformanceSummaryDto(
    int TotalQuestions,
    int AnsweredQuestions,
    int CorrectAnswers,
    int WrongAnswers,
    int PendingEssays);
