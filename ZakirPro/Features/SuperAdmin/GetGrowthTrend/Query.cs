using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.SuperAdmin.GetGrowthTrend;

public record Query(string Period) : IRequest<EndpointResponse<GrowthTrendDto>>;

public record GrowthTrendDto(
    string   Period,
    DateOnly SeriesStartDate,
    DateOnly SeriesEndDate,
    List<GrowthDataPoint> Series);

public record GrowthDataPoint(
    DateOnly Date,
    int      NewStudents,
    int      NewTeachers);
