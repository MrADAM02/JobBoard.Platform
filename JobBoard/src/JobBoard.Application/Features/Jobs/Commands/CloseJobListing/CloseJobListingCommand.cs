using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using MediatR;

namespace JobBoard.Application.Features.Jobs.Commands.CloseJobListing;

public record CloseJobListingCommand(Guid Id) : IRequest;

public class CloseJobListingCommandHandler : IRequestHandler<CloseJobListingCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CloseJobListingCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(CloseJobListingCommand request, CancellationToken cancellationToken)
    {
        var listing = _db.JobListings.FirstOrDefault(j => j.Id == request.Id)
            ?? throw new NotFoundException(nameof(JobListing), request.Id);

        var company = _db.Companies.First(c => c.Id == listing.CompanyId);
        if (company.OwnerUserId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException();
        }

        listing.Status = JobStatus.Closed;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
