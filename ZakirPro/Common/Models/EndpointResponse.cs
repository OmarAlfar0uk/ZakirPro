namespace ZakirPro.Common.Models;

public class EndpointResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static EndpointResponse<T> SuccessResponse(T? data, string message = "Success")
        => new() { Success = true, Message = message, Data = data };

    public static EndpointResponse<T> NotFoundResponse(string message = "Resource not found.")
        => new() { Success = false, Message = message };

    public static EndpointResponse<T> ErrorResponse(string message)
        => new() { Success = false, Message = message };

    public static EndpointResponse<T> ValidationErrorResponse(IEnumerable<string> errors)
        => new() { Success = false, Message = "Validation failed.", Errors = errors };

    public static EndpointResponse<T> ForbiddenResponse(string message = "Access denied.")
        => new() { Success = false, Message = message };

    public static EndpointResponse<T> UnauthorizedResponse(string message = "Unauthorized.")
        => new() { Success = false, Message = message };
}
