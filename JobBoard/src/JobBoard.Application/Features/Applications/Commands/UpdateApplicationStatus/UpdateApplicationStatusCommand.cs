using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Applications.Commands.UpdateApplicationStatus;

public record UpdateApplicationStatusCommand(Guid ApplicationId, ApplicationStatus NewStatus) : IRequest;

public class UpdateApplicationStatusCommandHandler : IRequestHandler<UpdateApplicationStatusCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService _emailService;

    public UpdateApplicationStatusCommandHandler(
        IApplicationDbContext db, ICurrentUserService currentUser, IEmailService emailService)
    {
        _db = db;
        _currentUser = currentUser;
        _emailService = emailService;
    }

    public async Task Handle(UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        var application = await _db.JobApplications
            .Include(a => a.JobListing).ThenInclude(j => j.Company)
            .Include(a => a.CandidateProfile).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException(nameof(JobApplication), request.ApplicationId);

        if (application.JobListing.Company.OwnerUserId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException("You do not own the job listing this application belongs to.");
        }

        application.Status = request.NewStatus;
        await _db.SaveChangesAsync(cancellationToken);

        await _emailService.SendAsync(
            application.CandidateProfile.User.Email,
            $"Update on your application for {application.JobListing.Title}",
            $"Your application status changed to: {request.NewStatus}.",
            cancellationToken);
    }
}
