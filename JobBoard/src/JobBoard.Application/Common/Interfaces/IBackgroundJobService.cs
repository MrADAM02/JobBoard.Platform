using System.Linq.Expressions;

namespace JobBoard.Application.Common.Interfaces;

// Thin abstraction over Hangfire's static BackgroundJob.Enqueue so the
// Application layer never references Hangfire directly - mirrors the existing
// IEmailService/IFileStorageService pattern of framework-free handlers.
public interface IBackgroundJobService
{
    void Enqueue<T>(Expression<Func<T, Task>> methodCall);
}
