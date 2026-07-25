using JobBoard.Application.Common.Interfaces;
using JobBoard.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Companies.Queries.GetCompanies;

// Backs a public company directory - mirrors GetJobListingsQuery's pattern
// (paginated, optional keyword filter), scoped to a much simpler shape since
// companies only need name-based search.
public record GetCompaniesQuery(string? Keyword, int PageNumber = 1, int PageSize = 20)
    : IRequest<PaginatedList<CompanySummaryDto>>;

public record CompanySummaryDto(
    Guid Id,
    string Name,
    string? LogoUrl,
    string? Location,
    string? Website,
    int OpenJobCount);

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, PaginatedList<CompanySummaryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetCompaniesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<CompanySummaryDto>> Handle(
        GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Companies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(c => c.Name.Contains(keyword));
        }

        var projected = query
            .OrderBy(c => c.Name)
            .Select(c => new CompanySummaryDto(
                c.Id, c.Name, c.LogoUrl, c.Location, c.Website,
                c.JobListings.Count(j => j.Status == Domain.Enums.JobStatus.Published)));

        return await PaginatedList<CompanySummaryDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
    }
}
