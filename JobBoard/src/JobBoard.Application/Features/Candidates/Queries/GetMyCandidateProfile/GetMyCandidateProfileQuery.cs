using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Candidates.Queries.GetMyCandidateProfile;

public record GetMyCandidateProfileQuery : IRequest<CandidateProfileDto>;

public record CandidateProfileDto(
    Guid Id, string FullName, string? Headline, string? Bio, string? ResumeUrl, string? Skills);

public class GetMyCandidateProfileQueryHandler
    : IRequestHandler<GetMyCandidateProfileQuery, CandidateProfileDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyCandidateProfileQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CandidateProfileDto> Handle(
        GetMyCandidateProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _db.CandidateProfiles.AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(CandidateProfile), _currentUser.UserId!);

        return new CandidateProfileDto(
            profile.Id, profile.FullName, profile.Headline, profile.Bio, profile.ResumeUrl, profile.Skills);
    }
}
