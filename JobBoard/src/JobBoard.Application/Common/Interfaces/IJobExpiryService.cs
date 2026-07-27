namespace JobBoard.Application.Common.Interfaces;

// Target of the Hangfire recurring job registered in Program.cs - kept as a
// plain interface (not a MediatR command) since Hangfire needs a concrete,
// parameterless method to target with RecurringJob.AddOrUpdate<T>.
public interface IJobExpiryService
{
    Task CloseExpiredJobsAsync();
}
