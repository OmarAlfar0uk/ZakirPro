using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Students.Exams.ListExams;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/students/{studentId:guid}/exams",
            async (Guid studentId, string? tab, ISender sender) =>
            {
                var result = await sender.Send(new Query(studentId, tab));

                if (!result.Success)
                {
                    if (result.Message.Contains("not authorized"))
                        return Results.Forbid();
                    return Results.BadRequest(result);
                }

                return Results.Ok(result);
            })
            .WithName("ListStudentExams")
            .WithTags("Students - Exams")
            .RequireAuthorization("StudentOnly");
    }
}
