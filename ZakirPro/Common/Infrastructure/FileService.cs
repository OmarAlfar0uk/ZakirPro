using ZakirPro.Common.Abstractions;

namespace ZakirPro.Common.Infrastructure;

public class FileService : IFileService
{
    private static readonly HashSet<string> AllowedExtensions =
        [".jpg", ".jpeg", ".png", ".webp"];

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
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException(
                $"File type '{ext}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

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
            var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
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
        return AllowedExtensions.Contains(ext);
    }

    public string BuildUrl(string relativePath)
    {
        var req = _httpContextAccessor.HttpContext?.Request;
        if (req is null) return relativePath;
        return $"{req.Scheme}://{req.Host}/{relativePath.TrimStart('/')}";
    }
}
