using FluentAssertions;
using FluentValidation;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Application.Features.Applications.Commands.ApplyToJob;
using JobBoard.Application.UnitTests.TestHelpers;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using JobBoard.Infrastructure.Persistence;
using NSubstitute;

namespace JobBoard.Application.UnitTests.Features.Applications;

public class ApplyToJobCommandHandlerTests
{
    private static (ApplyToJobCommandHandler handler, ApplicationDbContext db, Guid candidateUserId, Guid jobId)
        Setup(JobStatus jobStatus = JobStatus.Published)
    {
        var db = TestDbContextFactory.Create();

        var employer = new User { Email = "employer@test.com", PasswordHash = "x", Role = UserRole.Employer };
        var company = new Company { OwnerUser = employer, OwnerUserId = employer.Id, Name = "Acme" };
        var job = new JobListing
        {
            Company = company, CompanyId = company.Id, Title = "Backend Engineer",
            Description = "Build APIs", Location = "Remote", JobType = JobType.FullTime, Status = jobStatus
        };

        var candidateUser = new User { Email = "candidate@test.com", PasswordHash = "x", Role = UserRole.Candidate };
        var candidateProfile = new CandidateProfile { User = candidateUser, UserId = candidateUser.Id, FullName = "Cand One" };

        db.Users.AddRange(employer, candidateUser);
        db.Companies.Add(company);
        db.JobListings.Add(job);
        db.CandidateProfiles.Add(candidateProfile);
        db.SaveChanges();

        var currentUser = new FakeCurrentUserService { UserId = candidateUser.Id, Role = "Candidate" };
        var emailService = Substitute.For<IEmailService>();

        var handler = new ApplyToJobCommandHandler(db, currentUser, emailService);
        return (handler, db, candidateUser.Id, job.Id);
    }

    [Fact]
    public async Task Applying_to_a_published_job_creates_an_application()
    {
        var (handler, db, _, jobId) = Setup();

        var applicationId = await handler.Handle(new ApplyToJobCommand(jobId, "Cover letter"), CancellationToken.None);

        db.JobApplications.Should().ContainSingle(a => a.Id == applicationId);
    }

    [Fact]
    public async Task Applying_twice_to_the_same_job_throws()
    {
        var (handler, _, _, jobId) = Setup();
        await handler.Handle(new ApplyToJobCommand(jobId, "First application"), CancellationToken.None);

        var act = () => handler.Handle(new ApplyToJobCommand(jobId, "Second attempt"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Applying_to_a_non_published_job_throws()
    {
        var (handler, _, _, jobId) = Setup(JobStatus.Draft);

        var act = () => handler.Handle(new ApplyToJobCommand(jobId, "Cover letter"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
