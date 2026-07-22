using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using MediatR;

namespace JobBoard.Application.Features.Jobs.Commands.UpdateJobListing;

public record UpdateJobListingCommand(
    Guid Id, string Title, string Description, string Location, bool IsRemote,
    decimal? SalaryMin, decimal? SalaryMax, JobType JobType, string? Tags) : IRequest;

public class UpdateJobListingCommandHandler : IRequestHandler<UpdateJobListingCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateJobListingCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateJobListingCommand request, CancellationToken cancellationToken)
    {
        var listing = _db.JobListings.FirstOrDefault(j => j.Id == request.Id)
            ?? throw new NotFoundException(nameof(JobListing), request.Id);

        var company = _db.Companies.First(c => c.Id == listing.CompanyId);
        if (company.OwnerUserId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException();
        }

        listing.Title = request.Title;
        listing.Description = request.Description;
        listing.Location = request.Location;
        listing.IsRemote = request.IsRemote;
        listing.SalaryMin = request.SalaryMin;
        listing.SalaryMax = request.SalaryMax;
        listing.JobType = request.JobType;
        listing.Tags = request.Tags;
        listing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }
}
