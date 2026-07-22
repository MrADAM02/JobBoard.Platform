using FluentValidation;
using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Candidates.Commands.UpdateCandidateProfile;

// CandidateProfile itself is created automatically at registration (see RegisterCommandHandler) -
// this only ever edits the existing one for the current user.
public record UpdateCandidateProfileCommand(string FullName, string? Headline, string? Bio, string? Skills)
    : IRequest;

public class UpdateCandidateProfileCommandValidator : AbstractValidator<UpdateCandidateProfileCommand>
{
    public UpdateCandidateProfileCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Headline).MaximumLength(200);
    }
}

public class UpdateCandidateProfileCommandHandler : IRequestHandler<UpdateCandidateProfileCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateCandidateProfileCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateCandidateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _db.CandidateProfiles
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(CandidateProfile), _currentUser.UserId!);

        profile.FullName = request.FullName;
        profile.Headline = request.Headline;
        profile.Bio = request.Bio;
        profile.Skills = request.Skills;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }
}
