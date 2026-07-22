using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Applications.Queries.GetMyApplications;

// Powers the candidate's "my applications" tracker.
public record GetMyApplicationsQuery : IRequest<List<MyApplicationDto>>;

public record MyApplicationDto(
    Guid Id, Guid JobListingId, string JobTitle, string CompanyName, ApplicationStatus Status, DateTime AppliedAt);

public class GetMyApplicationsQueryHandler : IRequestHandler<GetMyApplicationsQuery, List<MyApplicationDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyApplicationsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<MyApplicationDto>> Handle(GetMyApplicationsQuery request, CancellationToken cancellationToken)
    {
        return await _db.JobApplications
            .AsNoTracking()
            .Where(a => a.CandidateProfile.UserId == _currentUser.UserId)
            .Include(a => a.JobListing).ThenInclude(j => j.Company)
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new MyApplicationDto(
                a.Id, a.JobListingId, a.JobListing.Title, a.JobListing.Company.Name, a.Status, a.AppliedAt))
            .ToListAsync(cancellationToken);
    }
}
