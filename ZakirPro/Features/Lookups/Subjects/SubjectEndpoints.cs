using MediatR;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Lookups.Subjects;

public class SubjectEndpoints : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        // POST /api/v1/lookups/subjects — Create a new subject (AdminOrAbove)
        app.MapPost("/api/v1/lookups/subjects",
            async (CreateSubjectCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.Success
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            })
            .WithName("CreateSubject")
            .WithTags("Lookups")
            .RequireAuthorization("AdminOrAbove");

        // PUT /api/v1/lookups/subjects/{id} — Update a subject (AdminOrAbove)
        app.MapPut("/api/v1/lookups/subjects/{id:guid}",
            async (Guid id, UpdateSubjectRequest body, ISender sender) =>
            {
                var result = await sender.Send(new UpdateSubjectCommand(id, body.Name));
                if (!result.Success)
                {
                    return result.Message.Contains("not found")
                        ? Results.NotFound(result)
                        : Results.BadRequest(result);
                }
                return Results.Ok(result);
            })
            .WithName("UpdateSubject")
            .WithTags("Lookups")
            .RequireAuthorization("AdminOrAbove");

        // DELETE /api/v1/lookups/subjects/{id} — Soft-delete a subject (AdminOrAbove)
        app.MapDelete("/api/v1/lookups/subjects/{id:guid}",
            async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteSubjectCommand(id));
                if (!result.Success)
                {
                    return result.Message.Contains("not found")
                        ? Results.NotFound(result)
                        : Results.BadRequest(result);
                }
                return Results.Ok(result);
            })
            .WithName("DeleteSubject")
            .WithTags("Lookups")
            .RequireAuthorization("AdminOrAbove");

        // GET /api/v1/lookups/subjects — Paginated list (TeacherOrAbove)
        app.MapGet("/api/v1/lookups/subjects",
            async (int page, int pageSize, ISender sender) =>
            {
                var query = new GetSubjectsQuery(
                    page > 0 ? page : 1,
                    pageSize > 0 ? pageSize : 50);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetSubjects")
            .WithTags("Lookups")
            .RequireAuthorization("TeacherOrAbove");
    }
}

/// <summary>Request body for PUT (id comes from route, so body only carries Name).</summary>
internal record UpdateSubjectRequest(string Name);
