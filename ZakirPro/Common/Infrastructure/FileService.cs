using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Common.Infrastructure;

public class FileService : IFileService
{
    private static readonly HashSet<string> AllowedImageExtensions =
        [".jpg", ".jpeg", ".png", ".webp"];

    private static readonly HashSet<string> AllowedAssignmentExtensions =
        [".pdf", ".doc", ".docx", ".txt", ".pptx", ".xlsx", ".jpg", ".jpeg", ".png"];

    public const long MaxAssignmentFileSizeBytes = 20 * 1024 * 1024; // 20 MB

    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<FileService> _logger;

    public FileService(
        IWebHostEnvironment env,
        IHttpContextAccessor httpContextAccessor,
        ILogger<FileService> logger)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<string> SaveImageAsync(IFormFile file, string subFolder = "images")
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
            throw new InvalidOperationException(
                $"File type '{ext}' is not allowed for images. Allowed types: {string.Join(", ", AllowedImageExtensions)}");

        var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", subFolder);
        Directory.CreateDirectory(uploadFolder);

        var storedName = $"{Guid.NewGuid()}{ext}";
        var filePath   = Path.Combine(uploadFolder, storedName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return BuildUrl($"uploads/{subFolder}/{storedName}");
    }

    public async Task<string> SaveAssignmentFileAsync(IFormFile file, string subFolder = "assignments")
    {
        if (file.Length > MaxAssignmentFileSizeBytes)
            throw new InvalidOperationException($"File size exceeds the maximum allowed limit of 20MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedAssignmentExtensions.Contains(ext))
            throw new InvalidOperationException(
                $"File type '{ext}' is not allowed for assignment submissions. Allowed types: {string.Join(", ", AllowedAssignmentExtensions)}");

        var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", subFolder);
        Directory.CreateDirectory(uploadFolder);

        var storedName = $"{Guid.NewGuid()}{ext}";
        var filePath   = Path.Combine(uploadFolder, storedName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return BuildUrl($"uploads/{subFolder}/{storedName}");
    }

    public void DeleteFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;
        try
        {
            // If full URL was passed, extract relative path
            var path = relativePath;
            if (Uri.TryCreate(relativePath, UriKind.Absolute, out var uri))
            {
                path = uri.AbsolutePath;
            }

            var fullPath = Path.Combine(_env.WebRootPath, path.TrimStart('/'));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file at {Path}", relativePath);
        }
    }

    public bool IsValidImageExtension(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedImageExtensions.Contains(ext);
    }

    public bool IsValidAssignmentExtension(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedAssignmentExtensions.Contains(ext);
    }

    public bool IsValidAssignmentFileSize(IFormFile file)
    {
        return file.Length > 0 && file.Length <= MaxAssignmentFileSizeBytes;
    }

    public string BuildUrl(string relativePath)
    {
        var req = _httpContextAccessor.HttpContext?.Request;
        if (req is null) return relativePath;
        return $"{req.Scheme}://{req.Host}/{relativePath.TrimStart('/')}";
    }
}
