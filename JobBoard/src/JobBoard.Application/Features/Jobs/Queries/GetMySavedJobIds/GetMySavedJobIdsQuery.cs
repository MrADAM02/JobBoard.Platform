using JobBoard.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Jobs.Queries.GetMySavedJobIds;

// Lightweight companion to GetMySavedJobsQuery - lets the frontend show the
// correct bookmark-toggle state on /jobs and /jobs/[id] without re-fetching
// full job data, mirroring how ApplyToJobBox already checks "already applied"
// via a full list fetched once on mount.
public record GetMySavedJobIdsQuery : IRequest<List<Guid>>;

public class GetMySavedJobIdsQueryHandler : IRequestHandler<GetMySavedJobIdsQuery, List<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMySavedJobIdsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<Guid>> Handle(GetMySavedJobIdsQuery request, CancellationToken cancellationToken)
    {
        return await _db.SavedJobs
            .AsNoTracking()
            .Where(s => s.UserId == _currentUser.UserId)
            .Select(s => s.JobListingId)
            .ToListAsync(cancellationToken);
    }
}
