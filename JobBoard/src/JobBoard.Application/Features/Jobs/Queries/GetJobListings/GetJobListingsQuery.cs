using JobBoard.Application.Common.Models;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using JobBoard.Application.Common.Interfaces;

namespace JobBoard.Application.Features.Jobs.Queries.GetJobListings;

// Backs the public, SSR-rendered /jobs listing page — supports the filters
// a candidate actually uses (keyword, location, type, salary floor).
public record GetJobListingsQuery(
    string? Keyword,
    string? Location,
    JobType? JobType,
    bool? RemoteOnly,
    decimal? MinSalary,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedList<JobListingSummaryDto>>;

public record JobListingSummaryDto(
    Guid Id,
    string Title,
    string CompanyName,
    string? CompanyLogoUrl,
    string Location,
    bool IsRemote,
    decimal? SalaryMin,
    decimal? SalaryMax,
    JobType JobType,
    DateTime? PublishedAt);

public class GetJobListingsQueryHandler
    : IRequestHandler<GetJobListingsQuery, PaginatedList<JobListingSummaryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetJobListingsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedList<JobListingSummaryDto>> Handle(
        GetJobListingsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.JobListings
            .AsNoTracking()
            .Where(j => j.Status == JobStatus.Published);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(j => j.Title.Contains(keyword) || j.Description.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            query = query.Where(j => j.Location.Contains(request.Location));
        }

        if (request.JobType.HasValue)
        {
            query = query.Where(j => j.JobType == request.JobType.Value);
        }

        if (request.RemoteOnly == true)
        {
            query = query.Where(j => j.IsRemote);
        }

        if (request.MinSalary.HasValue)
        {
            query = query.Where(j => j.SalaryMax == null || j.SalaryMax >= request.MinSalary.Value);
        }

        var projected = query
            .OrderByDescending(j => j.PublishedAt)
            .Select(j => new JobListingSummaryDto(
                j.Id, j.Title, j.Company.Name, j.Company.LogoUrl, j.Location,
                j.IsRemote, j.SalaryMin, j.SalaryMax, j.JobType, j.PublishedAt));

        return await PaginatedList<JobListingSummaryDto>.CreateAsync(
            projected, request.PageNumber, request.PageSize);
    }
}
