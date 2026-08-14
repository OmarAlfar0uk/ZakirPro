using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Auth.BulkImportStudents;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/admin/students/bulk-import",
            async (HttpContext ctx, ISender sender) =>
            {
                var form = await ctx.Request.ReadFormAsync();
                var file = form.Files.GetFile("file");
                Guid.TryParse(form["teacherId"], out var teacherId);
                Guid.TryParse(form["subjectId"], out var subjectId);
                Guid.TryParse(form["stageId"], out var stageId);

                var command = new Command(file!, teacherId, subjectId, stageId);
                var result = await sender.Send(command);
                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
        .DisableAntiforgery()
        .RequireAuthorization("AdminOrAbove")
        .WithName("BulkImportStudents")
        .WithTags("Auth");
    }
}
