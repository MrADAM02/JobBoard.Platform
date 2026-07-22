using JobBoard.Application.Common.Interfaces;
using JobBoard.Application.Common.Models;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Jobs.Queries.GetMyJobListings;

// Backs the employer dashboard "my listings" view - unlike the public GetJobListingsQuery,
// this includes Draft/Closed listings and is scoped to the caller's own company.
public record GetMyJobListingsQuery(int PageNumber = 1, int PageSize = 20)
    : IRequest<PaginatedList<MyJobListingDto>>;

public record MyJobListingDto(
    Guid Id,
    string Title,
    string Location,
    bool IsRemote,
    JobStatus Status,
    int ViewCount,
    int ApplicationCount,
    DateTime? PublishedAt,
    DateTime CreatedAt);

public class GetMyJobListingsQueryHandler
    : IRequestHandler<GetMyJobListingsQuery, PaginatedList<MyJobListingDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyJobListingsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<MyJobListingDto>> Handle(
        GetMyJobListingsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.JobListings
            .AsNoTracking()
            .Where(j => j.Company.OwnerUserId == _currentUser.UserId && j.Status != JobStatus.Deleted)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new MyJobListingDto(
                j.Id, j.Title, j.Location, j.IsRemote, j.Status,
                j.ViewCount, j.Applications.Count, j.PublishedAt, j.CreatedAt));

        return await PaginatedList<MyJobListingDto>.CreateAsync(query, request.PageNumber, request.PageSize);
    }
}
