using FluentValidation;
using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using MediatR;

namespace JobBoard.Application.Features.Jobs.Commands.PublishJobListing;

// The other half of "save as draft" from CreateJobListingCommand - without
// this, a job saved as a draft (PublishImmediately: false) had no way to
// ever become visible on the public /jobs listing.
public record PublishJobListingCommand(Guid Id) : IRequest;

public class PublishJobListingCommandHandler : IRequestHandler<PublishJobListingCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public PublishJobListingCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(PublishJobListingCommand request, CancellationToken cancellationToken)
    {
        var listing = _db.JobListings.FirstOrDefault(j => j.Id == request.Id)
            ?? throw new NotFoundException(nameof(JobListing), request.Id);

        var company = _db.Companies.First(c => c.Id == listing.CompanyId);
        if (company.OwnerUserId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException();
        }

        if (listing.Status != JobStatus.Draft)
        {
            throw new ValidationException("Only a draft listing can be published.");
        }

        listing.Status = JobStatus.Published;
        listing.PublishedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
