using FluentValidation;
using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Applications.Commands.SetApplicationNote;

// Private to the employer - never returned by GetMyApplicationsQuery (the
// candidate-facing query), only by GetApplicationsForJobQuery.
public record SetApplicationNoteCommand(Guid ApplicationId, string? Note) : IRequest;

public class SetApplicationNoteCommandValidator : AbstractValidator<SetApplicationNoteCommand>
{
    public SetApplicationNoteCommandValidator()
    {
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}

public class SetApplicationNoteCommandHandler : IRequestHandler<SetApplicationNoteCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public SetApplicationNoteCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(SetApplicationNoteCommand request, CancellationToken cancellationToken)
    {
        var application = await _db.JobApplications
            .Include(a => a.JobListing).ThenInclude(j => j.Company)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException(nameof(JobApplication), request.ApplicationId);

        if (application.JobListing.Company.OwnerUserId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException("You do not own the job listing this application belongs to.");
        }

        application.EmployerNotes = request.Note;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
