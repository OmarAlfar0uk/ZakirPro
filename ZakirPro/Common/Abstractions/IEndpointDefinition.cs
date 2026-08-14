namespace ZakirPro.Common.Abstractions;

public interface IEndpointDefinition
{
    void DefineEndpoints(IEndpointRouteBuilder app);
}
