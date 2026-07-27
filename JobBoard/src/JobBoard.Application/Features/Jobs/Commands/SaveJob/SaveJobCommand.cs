using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Jobs.Commands.SaveJob;

public record SaveJobCommand(Guid JobListingId) : IRequest;

public class SaveJobCommandHandler : IRequestHandler<SaveJobCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public SaveJobCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(SaveJobCommand request, CancellationToken cancellationToken)
    {
        var jobExists = await _db.JobListings.AnyAsync(j => j.Id == request.JobListingId, cancellationToken);
        if (!jobExists)
        {
            throw new NotFoundException(nameof(JobListing), request.JobListingId);
        }

        var alreadySaved = await _db.SavedJobs.AnyAsync(
            s => s.UserId == _currentUser.UserId && s.JobListingId == request.JobListingId, cancellationToken);

        // Idempotent - saving an already-saved job is a no-op, not an error
        // (the frontend toggle button can fire this without checking state first).
        if (alreadySaved)
        {
            return;
        }

        _db.SavedJobs.Add(new SavedJob
        {
            UserId = _currentUser.UserId!.Value,
            JobListingId = request.JobListingId
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
