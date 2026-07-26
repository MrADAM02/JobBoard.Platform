using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Jobs.Queries.GetJobLocations;

// Backs the /jobs location filter - a free-text box lets candidates type
// locations that match nothing, so the UI instead needs the actual distinct
// set of locations currently in use by published jobs, to render as a select.
public record GetJobLocationsQuery : IRequest<List<string>>;

public class GetJobLocationsQueryHandler : IRequestHandler<GetJobLocationsQuery, List<string>>
{
    private readonly IApplicationDbContext _db;

    public GetJobLocationsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<string>> Handle(GetJobLocationsQuery request, CancellationToken cancellationToken)
    {
        return await _db.JobListings
            .AsNoTracking()
            .Where(j => j.Status == JobStatus.Published)
            .Select(j => j.Location)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync(cancellationToken);
    }
}
