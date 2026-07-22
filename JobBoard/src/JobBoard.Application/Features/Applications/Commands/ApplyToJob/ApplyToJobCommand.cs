using FluentValidation;
using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Applications.Commands.ApplyToJob;

public record ApplyToJobCommand(Guid JobListingId, string? CoverLetter) : IRequest<Guid>;

public class ApplyToJobCommandValidator : AbstractValidator<ApplyToJobCommand>
{
    public ApplyToJobCommandValidator()
    {
        RuleFor(x => x.JobListingId).NotEmpty();
        RuleFor(x => x.CoverLetter).MaximumLength(4000);
    }
}

public class ApplyToJobCommandHandler : IRequestHandler<ApplyToJobCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService _emailService;

    public ApplyToJobCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IEmailService emailService)
    {
        _db = db;
        _currentUser = currentUser;
        _emailService = emailService;
    }

    public async Task<Guid> Handle(ApplyToJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _db.JobListings.Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == request.JobListingId, cancellationToken)
            ?? throw new NotFoundException(nameof(JobListing), request.JobListingId);

        if (job.Status != JobStatus.Published)
        {
            throw new ValidationException("This job is not accepting applications.");
        }

        var candidate = await _db.CandidateProfiles
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(CandidateProfile), _currentUser.UserId!);

        var alreadyApplied = await _db.JobApplications.AnyAsync(
            a => a.JobListingId == job.Id && a.CandidateProfileId == candidate.Id, cancellationToken);

        if (alreadyApplied)
        {
            throw new ValidationException("You have already applied to this job.");
        }

        var application = new JobApplication
        {
            JobListingId = job.Id,
            CandidateProfileId = candidate.Id,
            CoverLetter = request.CoverLetter,
            ResumeUrlSnapshot = candidate.ResumeUrl ?? string.Empty
        };

        _db.JobApplications.Add(application);
        await _db.SaveChangesAsync(cancellationToken);

        // Queue this via Hangfire in Infrastructure rather than awaiting inline -
        // an application shouldn't fail because the mail server is slow. See README.
        await _emailService.SendAsync(
            job.Company.OwnerUser?.Email ?? string.Empty,
            $"New application for {job.Title}",
            $"{candidate.FullName} just applied to {job.Title}.",
            cancellationToken);

        return application.Id;
    }
}
