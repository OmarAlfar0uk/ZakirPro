using MediatR;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Lookups.Stages;

public class StageEndpoints : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        // POST /api/v1/lookups/stages — Create a new stage (AdminOrAbove)
        app.MapPost("/api/v1/lookups/stages",
            async (CreateStageCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.Success
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            })
            .WithName("CreateStage")
            .WithTags("Lookups")
            .RequireAuthorization("AdminOrAbove");

        // PUT /api/v1/lookups/stages/{id} — Update a stage (AdminOrAbove)
        app.MapPut("/api/v1/lookups/stages/{id:guid}",
            async (Guid id, UpdateStageRequest body, ISender sender) =>
            {
                var result = await sender.Send(new UpdateStageCommand(id, body.Name, body.OrderIndex));
                if (!result.Success)
                {
                    return result.Message.Contains("not found")
                        ? Results.NotFound(result)
                        : Results.BadRequest(result);
                }
                return Results.Ok(result);
            })
            .WithName("UpdateStage")
            .WithTags("Lookups")
            .RequireAuthorization("AdminOrAbove");

        // DELETE /api/v1/lookups/stages/{id} — Soft-delete a stage (AdminOrAbove)
        app.MapDelete("/api/v1/lookups/stages/{id:guid}",
            async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteStageCommand(id));
                if (!result.Success)
                {
                    return result.Message.Contains("not found")
                        ? Results.NotFound(result)
                        : Results.BadRequest(result);
                }
                return Results.Ok(result);
            })
            .WithName("DeleteStage")
            .WithTags("Lookups")
            .RequireAuthorization("AdminOrAbove");

        // GET /api/v1/lookups/stages — Paginated list ordered by OrderIndex (TeacherOrAbove)
        app.MapGet("/api/v1/lookups/stages",
            async (int page, int pageSize, ISender sender) =>
            {
                var query = new GetStagesQuery(
                    page > 0 ? page : 1,
                    pageSize > 0 ? pageSize : 50);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetStages")
            .WithTags("Lookups")
            .RequireAuthorization("TeacherOrAbove");
    }
}

/// <summary>Request body for PUT (id comes from route).</summary>
internal record UpdateStageRequest(string Name, int OrderIndex);
