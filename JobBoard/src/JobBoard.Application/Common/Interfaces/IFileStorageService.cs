namespace JobBoard.Application.Common.Interfaces;

public interface IFileStorageService
{
    // Returns a public (or signed) URL for the stored file.
    // Local implementation writes to wwwroot/uploads; swap for S3/Azure Blob
    // implementations behind this same interface without touching callers.
    Task<string> SaveAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
}
