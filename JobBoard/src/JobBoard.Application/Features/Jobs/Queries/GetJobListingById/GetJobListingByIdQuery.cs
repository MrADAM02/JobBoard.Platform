using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Jobs.Queries.GetJobListingById;

public record GetJobListingByIdQuery(Guid Id) : IRequest<JobListingDetailDto>;

public record JobListingDetailDto(
    Guid Id,
    string Title,
    string Description,
    string CompanyName,
    string? CompanyLogoUrl,
    string? CompanyWebsite,
    string Location,
    bool IsRemote,
    decimal? SalaryMin,
    decimal? SalaryMax,
    JobType JobType,
    string? Tags,
    DateTime? PublishedAt);

public class GetJobListingByIdQueryHandler : IRequestHandler<GetJobListingByIdQuery, JobListingDetailDto>
{
    private readonly IApplicationDbContext _db;

    public GetJobListingByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<JobListingDetailDto> Handle(GetJobListingByIdQuery request, CancellationToken cancellationToken)
    {
        var listing = await _db.JobListings
            .AsNoTracking()
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == request.Id && j.Status != JobStatus.Deleted, cancellationToken)
            ?? throw new NotFoundException(nameof(JobListing), request.Id);

        // Fire-and-forget style view counting would need a separate command in a
        // real system (queries shouldn't have side effects) - see README for the CQRS note.

        return new JobListingDetailDto(
            listing.Id, listing.Title, listing.Description,
            listing.Company.Name, listing.Company.LogoUrl, listing.Company.Website,
            listing.Location, listing.IsRemote, listing.SalaryMin, listing.SalaryMax,
            listing.JobType, listing.Tags, listing.PublishedAt);
    }
}
