using FluentAssertions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Application.Features.Auth.Commands.Login;
using JobBoard.Application.UnitTests.TestHelpers;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using JobBoard.Infrastructure.Services;
using NSubstitute;

namespace JobBoard.Application.UnitTests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly PasswordHasher _passwordHasher = new();

    private LoginCommandHandler CreateHandler(out Infrastructure.Persistence.ApplicationDbContext db)
    {
        db = TestDbContextFactory.Create();
        var jwtService = Substitute.For<IJwtService>();
        jwtService.GenerateAccessToken(Arg.Any<User>()).Returns("access-token");
        jwtService.GenerateRefreshToken().Returns("new-refresh-token");

        return new LoginCommandHandler(db, _passwordHasher, jwtService);
    }

    [Fact]
    public async Task Valid_credentials_return_tokens_and_persist_refresh_token()
    {
        var handler = CreateHandler(out var db);
        var user = new User
        {
            Email = "user@test.com",
            PasswordHash = _passwordHasher.Hash("Password123!"),
            Role = UserRole.Candidate
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await handler.Handle(new LoginCommand("user@test.com", "Password123!"), CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        user.RefreshToken.Should().Be("new-refresh-token");
        user.RefreshTokenExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Wrong_password_throws_unauthorized()
    {
        var handler = CreateHandler(out var db);
        db.Users.Add(new User
        {
            Email = "user@test.com",
            PasswordHash = _passwordHasher.Hash("Password123!"),
            Role = UserRole.Candidate
        });
        await db.SaveChangesAsync();

        var act = () => handler.Handle(new LoginCommand("user@test.com", "WrongPassword!"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Unknown_email_throws_unauthorized()
    {
        var handler = CreateHandler(out _);

        var act = () => handler.Handle(new LoginCommand("nobody@test.com", "Password123!"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
