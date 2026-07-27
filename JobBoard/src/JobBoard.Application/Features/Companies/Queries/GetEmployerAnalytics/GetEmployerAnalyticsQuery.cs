using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Companies.Queries.GetEmployerAnalytics;

// Returns null (not a 404) when the employer hasn't set up a company yet,
// same convention as GetMyCompanyQuery - that's an expected first-login state.
public record GetEmployerAnalyticsQuery : IRequest<EmployerAnalyticsDto?>;

public record EmployerAnalyticsDto(
    int TotalViews,
    int TotalApplications,
    int TotalJobsPosted,
    List<DailyViewsDto> ViewsByDay,
    List<StatusCountDto> ApplicationsByStatus,
    List<TopJobDto> TopJobs);

public record DailyViewsDto(DateOnly Date, int Count);

public record StatusCountDto(ApplicationStatus Status, int Count);

public record TopJobDto(Guid Id, string Title, int ViewCount, int ApplicationCount);

public class GetEmployerAnalyticsQueryHandler : IRequestHandler<GetEmployerAnalyticsQuery, EmployerAnalyticsDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetEmployerAnalyticsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EmployerAnalyticsDto?> Handle(GetEmployerAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var company = await _db.Companies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.OwnerUserId == _currentUser.UserId, cancellationToken);

        if (company is null)
        {
            return null;
        }

        var jobIds = await _db.JobListings
            .Where(j => j.CompanyId == company.Id && j.Status != JobStatus.Deleted)
            .Select(j => j.Id)
            .ToListAsync(cancellationToken);

        var totalViews = await _db.JobListings
            .Where(j => jobIds.Contains(j.Id))
            .SumAsync(j => j.ViewCount, cancellationToken);
        var totalApplications = await _db.JobApplications
            .CountAsync(a => jobIds.Contains(a.JobListingId), cancellationToken);

        // Zero-filled so the chart shows a real 30-day axis, not just the days
        // that happened to have a view.
        var since = DateTime.UtcNow.Date.AddDays(-29);
        var rawViews = await _db.JobViews
            .Where(v => jobIds.Contains(v.JobListingId) && v.ViewedAt >= since)
            .GroupBy(v => v.ViewedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var viewsByDay = Enumerable.Range(0, 30)
            .Select(offset => since.AddDays(offset))
            .Select(date => new DailyViewsDto(
                DateOnly.FromDateTime(date),
                rawViews.FirstOrDefault(v => v.Date == date)?.Count ?? 0))
            .ToList();

        var applicationsByStatus = await _db.JobApplications
            .Where(a => jobIds.Contains(a.JobListingId))
            .GroupBy(a => a.Status)
            .Select(g => new StatusCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var topJobs = await _db.JobListings
            .Where(j => jobIds.Contains(j.Id))
            .OrderByDescending(j => j.ViewCount)
            .Take(5)
            .Select(j => new TopJobDto(j.Id, j.Title, j.ViewCount, j.Applications.Count))
            .ToListAsync(cancellationToken);

        return new EmployerAnalyticsDto(
            totalViews, totalApplications, jobIds.Count, viewsByDay, applicationsByStatus, topJobs);
    }
}
