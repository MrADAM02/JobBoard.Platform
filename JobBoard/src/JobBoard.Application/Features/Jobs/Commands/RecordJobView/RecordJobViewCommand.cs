using JobBoard.Domain.Enums;
using JobBoard.Application.Common.Interfaces;
using MediatR;

namespace JobBoard.Application.Features.Jobs.Commands.RecordJobView;

// Incrementing ViewCount is a side effect and doesn't belong in
// GetJobListingByIdQuery (queries stay read-only) - this is the separate
// command that query's comments flagged as the eventual home for it. Called
// from the frontend on client-side mount, not as part of the SSR data fetch,
// so crawler hits don't inflate the count.
public record RecordJobViewCommand(Guid Id) : IRequest;

public class RecordJobViewCommandHandler : IRequestHandler<RecordJobViewCommand>
{
    private readonly IApplicationDbContext _db;

    public RecordJobViewCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(RecordJobViewCommand request, CancellationToken cancellationToken)
    {
        var listing = _db.JobListings.FirstOrDefault(j => j.Id == request.Id && j.Status != JobStatus.Deleted);
        if (listing is null)
        {
            return; // silently ignore - a view ping for a missing/deleted job isn't an error worth surfacing
        }

        listing.ViewCount++;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
