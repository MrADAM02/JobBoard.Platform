using JobBoard.Application.Common.Interfaces;
using JobBoard.Application.Features.Auth.Commands.Register;
using MediatR;

namespace JobBoard.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResultDto>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IApplicationDbContext db, IJwtService jwtService)
    {
        _db = db;
        _jwtService = jwtService;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = _db.Users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

        if (user is null || user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token is invalid or expired.");
        }

        // Rotate the refresh token on every use so a stolen token can't be replayed indefinitely.
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(newAccessToken, newRefreshToken, user.Id, user.Email, user.Role.ToString());
    }
}
