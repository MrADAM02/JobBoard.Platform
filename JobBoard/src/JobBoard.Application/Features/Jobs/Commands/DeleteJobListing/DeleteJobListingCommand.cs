using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using MediatR;

namespace JobBoard.Application.Features.Jobs.Commands.DeleteJobListing;

// Soft-delete: sets Status = Deleted rather than removing the row, since
// JobApplication has an FK to JobListing and we don't want to orphan applications.
public record DeleteJobListingCommand(Guid Id) : IRequest;

public class DeleteJobListingCommandHandler : IRequestHandler<DeleteJobListingCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteJobListingCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteJobListingCommand request, CancellationToken cancellationToken)
    {
        var listing = _db.JobListings.FirstOrDefault(j => j.Id == request.Id)
            ?? throw new NotFoundException(nameof(JobListing), request.Id);

        var company = _db.Companies.First(c => c.Id == listing.CompanyId);
        if (company.OwnerUserId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException();
        }

        listing.Status = JobStatus.Deleted;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
