using MediatR;
using Microsoft.AspNetCore.Http;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.BulkImportStudents;

public record Command(
    IFormFile File,
    Guid TeacherId,
    Guid SubjectId,
    Guid StageId)
    : IRequest<EndpointResponse<BulkImportResult>>;

public record BulkImportResult(
    int TotalRows,
    int SuccessCount,
    int FailedCount,
    List<FailedRow> FailedRows);

public record FailedRow(int RowNumber, List<string> Errors);
