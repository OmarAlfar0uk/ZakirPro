using Microsoft.AspNetCore.Http;

namespace ZakirPro.Common.Abstractions;

public interface IFileService
{
    Task<string> SaveImageAsync(IFormFile file, string subFolder = "images");
    Task<string> SaveAssignmentFileAsync(IFormFile file, string subFolder = "assignments");
    void DeleteFile(string? relativePath);
    bool IsValidImageExtension(IFormFile file);
    bool IsValidAssignmentExtension(IFormFile file);
    bool IsValidAssignmentFileSize(IFormFile file);
    string BuildUrl(string relativePath);
}
