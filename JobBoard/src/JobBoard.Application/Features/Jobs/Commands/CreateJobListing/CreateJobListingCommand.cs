using FluentValidation;
using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using MediatR;

namespace JobBoard.Application.Features.Jobs.Commands.CreateJobListing;

public record CreateJobListingCommand(
    Guid CompanyId,
    string Title,
    string Description,
    string Location,
    bool IsRemote,
    decimal? SalaryMin,
    decimal? SalaryMax,
    JobType JobType,
    string? Tags,
    bool PublishImmediately) : IRequest<Guid>;

public class CreateJobListingCommandValidator : AbstractValidator<CreateJobListingCommand>
{
    public CreateJobListingCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Location).NotEmpty();
        RuleFor(x => x.SalaryMax)
            .GreaterThanOrEqualTo(x => x.SalaryMin!.Value)
            .When(x => x.SalaryMin.HasValue && x.SalaryMax.HasValue)
            .WithMessage("SalaryMax must be greater than or equal to SalaryMin.");
    }
}

public class CreateJobListingCommandHandler : IRequestHandler<CreateJobListingCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateJobListingCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateJobListingCommand request, CancellationToken cancellationToken)
    {
        var company = _db.Companies.FirstOrDefault(c => c.Id == request.CompanyId)
            ?? throw new NotFoundException(nameof(Company), request.CompanyId);

        // Only the company's owner (the employer who created it) can post jobs under it.
        if (company.OwnerUserId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException("You do not own this company profile.");
        }

        var listing = new JobListing
        {
            CompanyId = request.CompanyId,
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            IsRemote = request.IsRemote,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            JobType = request.JobType,
            Tags = request.Tags,
            Status = request.PublishImmediately ? JobStatus.Published : JobStatus.Draft,
            PublishedAt = request.PublishImmediately ? DateTime.UtcNow : null
        };

        _db.JobListings.Add(listing);
        await _db.SaveChangesAsync(cancellationToken);

        return listing.Id;
    }
}
