using JobBoard.Application.Common.Interfaces;
using JobBoard.Application.Common.Models;
using JobBoard.Application.Features.Jobs.Queries.GetJobListings;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Jobs.Queries.GetMySavedJobs;

// Reuses GetJobListingsQuery's JobListingSummaryDto - same card shape as the
// public /jobs listing, just filtered through the caller's SavedJob rows.
// Excludes only Deleted listings; a saved job that's since closed/expired
// still shows (silently dropping it would be more surprising than useful).
public record GetMySavedJobsQuery(int PageNumber = 1, int PageSize = 20)
    : IRequest<PaginatedList<JobListingSummaryDto>>;

public class GetMySavedJobsQueryHandler
    : IRequestHandler<GetMySavedJobsQuery, PaginatedList<JobListingSummaryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMySavedJobsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<JobListingSummaryDto>> Handle(
        GetMySavedJobsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.SavedJobs
            .AsNoTracking()
            .Where(s => s.UserId == _currentUser.UserId && s.JobListing.Status != JobStatus.Deleted)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new JobListingSummaryDto(
                s.JobListing.Id, s.JobListing.Title, s.JobListing.Company.Name, s.JobListing.Company.LogoUrl,
                s.JobListing.Location, s.JobListing.IsRemote, s.JobListing.SalaryMin, s.JobListing.SalaryMax,
                s.JobListing.JobType, s.JobListing.PublishedAt));

        return await PaginatedList<JobListingSummaryDto>.CreateAsync(query, request.PageNumber, request.PageSize);
    }
}
