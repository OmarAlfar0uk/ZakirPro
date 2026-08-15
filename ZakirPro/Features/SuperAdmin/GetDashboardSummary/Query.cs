using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.SuperAdmin.GetDashboardSummary;

public record Query : IRequest<EndpointResponse<DashboardSummaryDto>>;

public record DashboardSummaryDto(
    UserCountsDto   Users,
    ContentCountsDto Content,
    string[]        NotYetAvailable);

public record UserCountsDto(
    int TotalTeachers,
    int TotalStudents,
    int TotalAdmins,
    int TotalAssistants,
    int NewThisWeek,
    int NewThisMonth);

public record ContentCountsDto(
    int TotalTeacherSubjectStageAssignments,
    int TotalLectures,
    ExamCountsDto Exams);

public record ExamCountsDto(int Draft, int Published, int Archived);
