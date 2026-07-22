using JobBoard.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace JobBoard.Infrastructure.Services;

// Dev-friendly implementation that writes to wwwroot/uploads. Swap this
// registration for an S3FileStorageService/AzureBlobFileStorageService in
// DependencyInjection.cs when you deploy - callers never change.
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsRoot);

        var safeName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(uploadsRoot, safeName);

        await using var fileOut = File.Create(fullPath);
        await fileStream.CopyToAsync(fileOut, cancellationToken);

        return $"/uploads/{safeName}";
    }

    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(fileUrl);
        var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", fileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }
}
