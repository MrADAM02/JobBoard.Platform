using FluentAssertions;
using JobBoard.Application.Features.Jobs.Commands.SaveJob;
using JobBoard.Application.Features.Jobs.Commands.UnsaveJob;
using JobBoard.Application.UnitTests.TestHelpers;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using JobBoard.Infrastructure.Persistence;

namespace JobBoard.Application.UnitTests.Features.Jobs;

public class SaveJobCommandHandlerTests
{
    private static (ApplicationDbContext db, Guid candidateUserId, Guid jobId) Setup()
    {
        var db = TestDbContextFactory.Create();

        var employer = new User { Email = "employer@test.com", PasswordHash = "x", Role = UserRole.Employer };
        var company = new Company { OwnerUser = employer, OwnerUserId = employer.Id, Name = "Acme" };
        var job = new JobListing
        {
            Company = company, CompanyId = company.Id, Title = "Backend Engineer",
            Description = "Build APIs", Location = "Remote", JobType = JobType.FullTime, Status = JobStatus.Published
        };
        var candidateUser = new User { Email = "candidate@test.com", PasswordHash = "x", Role = UserRole.Candidate };

        db.Users.AddRange(employer, candidateUser);
        db.Companies.Add(company);
        db.JobListings.Add(job);
        db.SaveChanges();

        return (db, candidateUser.Id, job.Id);
    }

    [Fact]
    public async Task Saving_a_job_creates_a_saved_job_row()
    {
        var (db, candidateUserId, jobId) = Setup();
        var currentUser = new FakeCurrentUserService { UserId = candidateUserId, Role = "Candidate" };
        var handler = new SaveJobCommandHandler(db, currentUser);

        await handler.Handle(new SaveJobCommand(jobId), CancellationToken.None);

        db.SavedJobs.Should().ContainSingle(s => s.UserId == candidateUserId && s.JobListingId == jobId);
    }

    [Fact]
    public async Task Saving_the_same_job_twice_is_idempotent()
    {
        var (db, candidateUserId, jobId) = Setup();
        var currentUser = new FakeCurrentUserService { UserId = candidateUserId, Role = "Candidate" };
        var handler = new SaveJobCommandHandler(db, currentUser);

        await handler.Handle(new SaveJobCommand(jobId), CancellationToken.None);
        await handler.Handle(new SaveJobCommand(jobId), CancellationToken.None);

        db.SavedJobs.Should().ContainSingle(s => s.UserId == candidateUserId && s.JobListingId == jobId);
    }

    [Fact]
    public async Task Unsaving_removes_the_saved_job_row()
    {
        var (db, candidateUserId, jobId) = Setup();
        var currentUser = new FakeCurrentUserService { UserId = candidateUserId, Role = "Candidate" };
        var saveHandler = new SaveJobCommandHandler(db, currentUser);
        var unsaveHandler = new UnsaveJobCommandHandler(db, currentUser);
        await saveHandler.Handle(new SaveJobCommand(jobId), CancellationToken.None);

        await unsaveHandler.Handle(new UnsaveJobCommand(jobId), CancellationToken.None);

        db.SavedJobs.Should().BeEmpty();
    }

    [Fact]
    public async Task Unsaving_something_never_saved_does_not_throw()
    {
        var (db, candidateUserId, jobId) = Setup();
        var currentUser = new FakeCurrentUserService { UserId = candidateUserId, Role = "Candidate" };
        var handler = new UnsaveJobCommandHandler(db, currentUser);

        var act = () => handler.Handle(new UnsaveJobCommand(jobId), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
