using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Applications.Queries.GetApplicationsForJob;

// Powers the employer's "applicants for this listing" dashboard view.
public record GetApplicationsForJobQuery(Guid JobListingId) : IRequest<List<ApplicationSummaryDto>>;

public record ApplicationSummaryDto(
    Guid Id, string CandidateName, string? ResumeUrl, ApplicationStatus Status, DateTime AppliedAt);

public class GetApplicationsForJobQueryHandler
    : IRequestHandler<GetApplicationsForJobQuery, List<ApplicationSummaryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetApplicationsForJobQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<ApplicationSummaryDto>> Handle(
        GetApplicationsForJobQuery request, CancellationToken cancellationToken)
    {
        var job = await _db.JobListings.Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == request.JobListingId, cancellationToken)
            ?? throw new NotFoundException(nameof(JobListing), request.JobListingId);

        if (job.Company.OwnerUserId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException("You do not own this job listing.");
        }

        return await _db.JobApplications
            .AsNoTracking()
            .Where(a => a.JobListingId == request.JobListingId)
            .Include(a => a.CandidateProfile)
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new ApplicationSummaryDto(
                a.Id, a.CandidateProfile.FullName, a.CandidateProfile.ResumeUrl, a.Status, a.AppliedAt))
            .ToListAsync(cancellationToken);
    }
}
