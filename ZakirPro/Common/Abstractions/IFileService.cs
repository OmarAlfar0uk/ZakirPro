namespace ZakirPro.Common.Abstractions;

public interface IFileService
{
    Task<string> SaveImageAsync(IFormFile file, string subFolder = "images");
    void DeleteFile(string? relativePath);
    bool IsValidImageExtension(IFormFile file);
    string BuildUrl(string relativePath);
}
