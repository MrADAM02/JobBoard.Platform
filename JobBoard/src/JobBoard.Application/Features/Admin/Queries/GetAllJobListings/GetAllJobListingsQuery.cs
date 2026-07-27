using JobBoard.Application.Common.Interfaces;
using JobBoard.Application.Common.Models;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Admin.Queries.GetAllJobListings;

// Admin-wide equivalent of GetMyJobListingsQuery - no ownership scope, spans
// every company, and (unlike the public GetJobListingsQuery) includes every
// status so moderation can see drafts/closed/expired listings too.
public record GetAllJobListingsQuery(JobStatus? Status, int PageNumber = 1, int PageSize = 20)
    : IRequest<PaginatedList<AdminJobListingDto>>;

public record AdminJobListingDto(
    Guid Id, string Title, string CompanyName, JobStatus Status, int ViewCount,
    int ApplicationCount, DateTime CreatedAt);

public class GetAllJobListingsQueryHandler
    : IRequestHandler<GetAllJobListingsQuery, PaginatedList<AdminJobListingDto>>
{
    private readonly IApplicationDbContext _db;

    public GetAllJobListingsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<AdminJobListingDto>> Handle(
        GetAllJobListingsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.JobListings.AsNoTracking().Where(j => j.Status != JobStatus.Deleted);

        if (request.Status.HasValue)
        {
            query = query.Where(j => j.Status == request.Status.Value);
        }

        var projected = query
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new AdminJobListingDto(
                j.Id, j.Title, j.Company.Name, j.Status, j.ViewCount, j.Applications.Count, j.CreatedAt));

        return await PaginatedList<AdminJobListingDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
