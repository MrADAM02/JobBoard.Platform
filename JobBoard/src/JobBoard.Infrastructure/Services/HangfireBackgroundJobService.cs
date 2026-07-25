using System.Linq.Expressions;
using Hangfire;
using JobBoard.Application.Common.Interfaces;

namespace JobBoard.Infrastructure.Services;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    public void Enqueue<T>(Expression<Func<T, Task>> methodCall) => BackgroundJob.Enqueue(methodCall);
}
