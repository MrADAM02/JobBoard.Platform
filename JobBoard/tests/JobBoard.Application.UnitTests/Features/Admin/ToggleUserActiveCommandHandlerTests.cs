using FluentAssertions;
using FluentValidation;
using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Features.Admin.Commands.ToggleUserActive;
using JobBoard.Application.UnitTests.TestHelpers;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using JobBoard.Infrastructure.Persistence;

namespace JobBoard.Application.UnitTests.Features.Admin;

public class ToggleUserActiveCommandHandlerTests
{
    private static (ApplicationDbContext db, User admin, User target) Seed()
    {
        var db = TestDbContextFactory.Create();

        var admin = new User { Email = "admin@test.com", PasswordHash = "x", Role = UserRole.Admin };
        var target = new User { Email = "candidate@test.com", PasswordHash = "x", Role = UserRole.Candidate, IsActive = true };

        db.Users.AddRange(admin, target);
        db.SaveChanges();

        return (db, admin, target);
    }

    [Fact]
    public async Task Toggling_flips_is_active()
    {
        var (db, admin, target) = Seed();
        var currentUser = new FakeCurrentUserService { UserId = admin.Id, Role = "Admin" };
        var handler = new ToggleUserActiveCommandHandler(db, currentUser);

        await handler.Handle(new ToggleUserActiveCommand(target.Id), CancellationToken.None);

        db.Users.First(u => u.Id == target.Id).IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Admin_cannot_deactivate_their_own_account()
    {
        var (db, admin, _) = Seed();
        var currentUser = new FakeCurrentUserService { UserId = admin.Id, Role = "Admin" };
        var handler = new ToggleUserActiveCommandHandler(db, currentUser);

        var act = () => handler.Handle(new ToggleUserActiveCommand(admin.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Unknown_user_throws_not_found()
    {
        var (db, admin, _) = Seed();
        var currentUser = new FakeCurrentUserService { UserId = admin.Id, Role = "Admin" };
        var handler = new ToggleUserActiveCommandHandler(db, currentUser);

        var act = () => handler.Handle(new ToggleUserActiveCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
