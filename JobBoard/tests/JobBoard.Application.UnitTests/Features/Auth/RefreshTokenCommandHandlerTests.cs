using FluentAssertions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Application.Features.Auth.Commands.RefreshToken;
using JobBoard.Application.UnitTests.TestHelpers;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using NSubstitute;

namespace JobBoard.Application.UnitTests.Features.Auth;

public class RefreshTokenCommandHandlerTests
{
    private RefreshTokenCommandHandler CreateHandler(out Infrastructure.Persistence.ApplicationDbContext db)
    {
        db = TestDbContextFactory.Create();
        var jwtService = Substitute.For<IJwtService>();
        jwtService.GenerateAccessToken(Arg.Any<User>()).Returns("new-access-token");
        jwtService.GenerateRefreshToken().Returns("rotated-refresh-token");

        return new RefreshTokenCommandHandler(db, jwtService);
    }

    [Fact]
    public async Task Valid_token_rotates_to_a_new_refresh_token()
    {
        var handler = CreateHandler(out var db);
        var user = new User
        {
            Email = "user@test.com",
            PasswordHash = "hash",
            Role = UserRole.Candidate,
            RefreshToken = "old-refresh-token",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await handler.Handle(new RefreshTokenCommand("old-refresh-token"), CancellationToken.None);

        result.RefreshToken.Should().Be("rotated-refresh-token").And.NotBe("old-refresh-token");
        user.RefreshToken.Should().Be("rotated-refresh-token");
    }

    [Fact]
    public async Task Expired_token_throws_unauthorized()
    {
        var handler = CreateHandler(out var db);
        db.Users.Add(new User
        {
            Email = "user@test.com",
            PasswordHash = "hash",
            Role = UserRole.Candidate,
            RefreshToken = "expired-token",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var act = () => handler.Handle(new RefreshTokenCommand("expired-token"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Unknown_token_throws_unauthorized()
    {
        var handler = CreateHandler(out _);

        var act = () => handler.Handle(new RefreshTokenCommand("does-not-exist"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
