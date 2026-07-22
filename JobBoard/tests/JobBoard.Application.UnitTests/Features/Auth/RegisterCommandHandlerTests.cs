using FluentAssertions;
using FluentValidation;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Application.Features.Auth.Commands.Register;
using JobBoard.Application.UnitTests.TestHelpers;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using JobBoard.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace JobBoard.Application.UnitTests.Features.Auth;

public class RegisterCommandHandlerTests
{
    private static RegisterCommandHandler CreateHandler(out Infrastructure.Persistence.ApplicationDbContext db)
    {
        db = TestDbContextFactory.Create();
        var jwtService = Substitute.For<IJwtService>();
        jwtService.GenerateAccessToken(Arg.Any<User>()).Returns("access-token");
        jwtService.GenerateRefreshToken().Returns("refresh-token");

        return new RegisterCommandHandler(db, new PasswordHasher(), jwtService);
    }

    [Fact]
    public async Task Registering_a_candidate_creates_a_candidate_profile()
    {
        var handler = CreateHandler(out var db);
        var command = new RegisterCommand("candidate@test.com", "Password123!", "Jane Candidate", UserRole.Candidate);

        var result = await handler.Handle(command, CancellationToken.None);

        var user = db.Users.Include(u => u.CandidateProfile).First(u => u.Id == result.UserId);
        user.CandidateProfile.Should().NotBeNull();
        user.CandidateProfile!.FullName.Should().Be("Jane Candidate");
        user.Company.Should().BeNull();
    }

    [Fact]
    public async Task Registering_an_employer_does_not_create_a_company()
    {
        var handler = CreateHandler(out var db);
        var command = new RegisterCommand("employer@test.com", "Password123!", "Jo Employer", UserRole.Employer);

        var result = await handler.Handle(command, CancellationToken.None);

        var user = db.Users.Include(u => u.Company).First(u => u.Id == result.UserId);
        user.Company.Should().BeNull();
        user.CandidateProfile.Should().BeNull();
    }

    [Fact]
    public async Task Registering_with_an_email_already_in_use_throws()
    {
        var handler = CreateHandler(out var db);
        db.Users.Add(new User { Email = "taken@test.com", PasswordHash = "x", Role = UserRole.Candidate });
        await db.SaveChangesAsync();

        var command = new RegisterCommand("taken@test.com", "Password123!", "Someone Else", UserRole.Candidate);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public void Validator_rejects_self_registering_as_admin()
    {
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand("admin@test.com", "Password123!", "Would-be Admin", UserRole.Admin);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
