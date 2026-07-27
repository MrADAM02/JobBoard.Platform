using JobBoard.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Jobs.Commands.UnsaveJob;

public record UnsaveJobCommand(Guid JobListingId) : IRequest;

public class UnsaveJobCommandHandler : IRequestHandler<UnsaveJobCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UnsaveJobCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(UnsaveJobCommand request, CancellationToken cancellationToken)
    {
        var saved = await _db.SavedJobs.FirstOrDefaultAsync(
            s => s.UserId == _currentUser.UserId && s.JobListingId == request.JobListingId, cancellationToken);

        // Idempotent, mirrors SaveJobCommand - unsaving something not saved is a no-op.
        if (saved is null)
        {
            return;
        }

        _db.SavedJobs.Remove(saved);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
