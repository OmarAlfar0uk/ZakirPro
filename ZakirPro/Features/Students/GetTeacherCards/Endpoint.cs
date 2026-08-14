using MediatR;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Students.GetTeacherCards;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/students/{studentId:guid}/teacher-cards",
            async (Guid studentId, ISender sender) =>
            {
                var result = await sender.Send(new Query(studentId));
                return result.Success
                    ? Results.Ok(result)
                    : result.Message.Contains("not authorized") || result.Message.Contains("Access denied")
                        ? Results.Forbid()
                        : Results.BadRequest(result);
            })
            .WithName("GetTeacherCards")
            .WithTags("Students")
            .RequireAuthorization("StudentOnly");
    }
}
