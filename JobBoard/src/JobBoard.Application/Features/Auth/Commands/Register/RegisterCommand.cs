using FluentValidation;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using MediatR;

namespace JobBoard.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password, string FullName, UserRole Role)
    : IRequest<AuthResultDto>;

public record AuthResultDto(string AccessToken, string RefreshToken, Guid UserId, string Email, string Role);

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).IsInEnum().NotEqual(UserRole.Admin); // admins are seeded/promoted, not self-registered
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(IApplicationDbContext db, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailInUse = await Task.FromResult(_db.Users.Any(u => u.Email == request.Email));
        if (emailInUse)
        {
            throw new ValidationException($"An account with email '{request.Email}' already exists.");
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role
        };

        if (request.Role == UserRole.Candidate)
        {
            user.CandidateProfile = new CandidateProfile { FullName = request.FullName };
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        // Employers create their Company separately via CreateCompanyCommand
        // after registering, since one user could in theory manage this later.

        return new AuthResultDto(accessToken, refreshToken, user.Id, user.Email, user.Role.ToString());
    }
}
