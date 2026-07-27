using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Admin.Queries.GetPlatformStats;

public record GetPlatformStatsQuery : IRequest<PlatformStatsDto>;

public record PlatformStatsDto(
    int TotalUsers, int TotalCandidates, int TotalEmployers,
    int TotalCompanies, int TotalJobListings, int TotalApplications);

public class GetPlatformStatsQueryHandler : IRequestHandler<GetPlatformStatsQuery, PlatformStatsDto>
{
    private readonly IApplicationDbContext _db;

    public GetPlatformStatsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PlatformStatsDto> Handle(GetPlatformStatsQuery request, CancellationToken cancellationToken)
    {
        var totalUsers = await _db.Users.CountAsync(cancellationToken);
        var totalCandidates = await _db.Users.CountAsync(u => u.Role == UserRole.Candidate, cancellationToken);
        var totalEmployers = await _db.Users.CountAsync(u => u.Role == UserRole.Employer, cancellationToken);
        var totalCompanies = await _db.Companies.CountAsync(cancellationToken);
        var totalJobListings = await _db.JobListings.CountAsync(j => j.Status != JobStatus.Deleted, cancellationToken);
        var totalApplications = await _db.JobApplications.CountAsync(cancellationToken);

        return new PlatformStatsDto(
            totalUsers, totalCandidates, totalEmployers, totalCompanies, totalJobListings, totalApplications);
    }
}
