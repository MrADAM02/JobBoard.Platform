using FluentAssertions;
using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Features.Jobs.Commands.DeleteJobListing;
using JobBoard.Application.UnitTests.TestHelpers;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using JobBoard.Infrastructure.Persistence;

namespace JobBoard.Application.UnitTests.Features.Jobs;

public class DeleteJobListingCommandHandlerTests
{
    private static (ApplicationDbContext db, JobListing job, User owningEmployer, User otherEmployer, User admin) Seed()
    {
        var db = TestDbContextFactory.Create();

        var owningEmployer = new User { Email = "owner@test.com", PasswordHash = "x", Role = UserRole.Employer };
        var otherEmployer = new User { Email = "other@test.com", PasswordHash = "x", Role = UserRole.Employer };
        var admin = new User { Email = "admin@test.com", PasswordHash = "x", Role = UserRole.Admin };
        var company = new Company { OwnerUser = owningEmployer, OwnerUserId = owningEmployer.Id, Name = "Acme" };
        var job = new JobListing
        {
            Company = company, CompanyId = company.Id, Title = "Backend Engineer",
            Description = "Build APIs", Location = "Remote", JobType = JobType.FullTime, Status = JobStatus.Published
        };

        db.Users.AddRange(owningEmployer, otherEmployer, admin);
        db.Companies.Add(company);
        db.JobListings.Add(job);
        db.SaveChanges();

        return (db, job, owningEmployer, otherEmployer, admin);
    }

    [Fact]
    public async Task Owning_employer_can_delete()
    {
        var (db, job, owningEmployer, _, _) = Seed();
        var currentUser = new FakeCurrentUserService { UserId = owningEmployer.Id, Role = "Employer" };
        var handler = new DeleteJobListingCommandHandler(db, currentUser);

        await handler.Handle(new DeleteJobListingCommand(job.Id), CancellationToken.None);

        db.JobListings.First(j => j.Id == job.Id).Status.Should().Be(JobStatus.Deleted);
    }

    [Fact]
    public async Task Non_owning_employer_is_forbidden()
    {
        var (db, job, _, otherEmployer, _) = Seed();
        var currentUser = new FakeCurrentUserService { UserId = otherEmployer.Id, Role = "Employer" };
        var handler = new DeleteJobListingCommandHandler(db, currentUser);

        var act = () => handler.Handle(new DeleteJobListingCommand(job.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Admin_can_delete_a_listing_they_do_not_own()
    {
        var (db, job, _, _, admin) = Seed();
        var currentUser = new FakeCurrentUserService { UserId = admin.Id, Role = "Admin" };
        var handler = new DeleteJobListingCommandHandler(db, currentUser);

        await handler.Handle(new DeleteJobListingCommand(job.Id), CancellationToken.None);

        db.JobListings.First(j => j.Id == job.Id).Status.Should().Be(JobStatus.Deleted);
    }
}
