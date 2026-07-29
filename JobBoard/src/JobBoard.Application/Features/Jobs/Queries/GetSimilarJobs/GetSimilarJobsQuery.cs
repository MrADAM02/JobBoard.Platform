using JobBoard.Application.Common.Interfaces;
using JobBoard.Application.Features.Jobs.Queries.GetJobListings;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Jobs.Queries.GetSimilarJobs;

// Backs the "similar jobs" section on the public job detail page - reuses
// JobListingSummaryDto (same shape the frontend job cards already render),
// no new DTO/type needed on either side.
public record GetSimilarJobsQuery(Guid JobId) : IRequest<List<JobListingSummaryDto>>;

public class GetSimilarJobsQueryHandler : IRequestHandler<GetSimilarJobsQuery, List<JobListingSummaryDto>>
{
    private const int MaxResults = 4;

    private readonly IApplicationDbContext _db;

    public GetSimilarJobsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<JobListingSummaryDto>> Handle(GetSimilarJobsQuery request, CancellationToken cancellationToken)
    {
        var target = await _db.JobListings
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == request.JobId && j.Status != JobStatus.Deleted, cancellationToken);

        // A missing/deleted job has no "similar" jobs worth surfacing - non-fatal,
        // same convention as RecordJobViewCommandHandler's silent-ignore.
        if (target is null)
        {
            return [];
        }

        return await _db.JobListings
            .AsNoTracking()
            .Where(j => j.Id != target.Id && j.Status == JobStatus.Published
                && (j.JobType == target.JobType || j.Location == target.Location))
            // Listings matching both type and location outrank a single match; ties broken by recency.
            .OrderByDescending(j => j.JobType == target.JobType && j.Location == target.Location)
            .ThenByDescending(j => j.PublishedAt)
            .Take(MaxResults)
            .Select(j => new JobListingSummaryDto(
                j.Id, j.Title, j.Company.Name, j.Company.LogoUrl, j.Location,
                j.IsRemote, j.SalaryMin, j.SalaryMax, j.JobType, j.PublishedAt))
            .ToListAsync(cancellationToken);
    }
}
