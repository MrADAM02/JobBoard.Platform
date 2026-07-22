using FluentAssertions;
using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Application.Features.Applications.Commands.UpdateApplicationStatus;
using JobBoard.Application.UnitTests.TestHelpers;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using JobBoard.Infrastructure.Persistence;
using NSubstitute;

namespace JobBoard.Application.UnitTests.Features.Applications;

public class UpdateApplicationStatusCommandHandlerTests
{
    private static (ApplicationDbContext db, JobApplication application, User owningEmployer, User otherEmployer) Seed()
    {
        var db = TestDbContextFactory.Create();

        var owningEmployer = new User { Email = "owner@test.com", PasswordHash = "x", Role = UserRole.Employer };
        var otherEmployer = new User { Email = "other@test.com", PasswordHash = "x", Role = UserRole.Employer };
        var company = new Company { OwnerUser = owningEmployer, OwnerUserId = owningEmployer.Id, Name = "Acme" };
        var job = new JobListing
        {
            Company = company, CompanyId = company.Id, Title = "Backend Engineer",
            Description = "Build APIs", Location = "Remote", JobType = JobType.FullTime, Status = JobStatus.Published
        };
        var candidateUser = new User { Email = "candidate@test.com", PasswordHash = "x", Role = UserRole.Candidate };
        var candidateProfile = new CandidateProfile { User = candidateUser, UserId = candidateUser.Id, FullName = "Cand One" };
        var application = new JobApplication
        {
            JobListing = job, JobListingId = job.Id,
            CandidateProfile = candidateProfile, CandidateProfileId = candidateProfile.Id,
            Status = ApplicationStatus.Applied, ResumeUrlSnapshot = string.Empty
        };

        db.Users.AddRange(owningEmployer, otherEmployer, candidateUser);
        db.Companies.Add(company);
        db.JobListings.Add(job);
        db.CandidateProfiles.Add(candidateProfile);
        db.JobApplications.Add(application);
        db.SaveChanges();

        return (db, application, owningEmployer, otherEmployer);
    }

    [Fact]
    public async Task Owning_employer_can_update_status()
    {
        var (db, application, owningEmployer, _) = Seed();
        var currentUser = new FakeCurrentUserService { UserId = owningEmployer.Id, Role = "Employer" };
        var emailService = Substitute.For<IEmailService>();
        var handler = new UpdateApplicationStatusCommandHandler(db, currentUser, emailService);

        await handler.Handle(new UpdateApplicationStatusCommand(application.Id, ApplicationStatus.InterviewScheduled), CancellationToken.None);

        db.JobApplications.First(a => a.Id == application.Id).Status.Should().Be(ApplicationStatus.InterviewScheduled);
        await emailService.Received(1).SendAsync(
            "candidate@test.com", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Non_owning_employer_is_forbidden()
    {
        var (db, application, _, otherEmployer) = Seed();
        var currentUser = new FakeCurrentUserService { UserId = otherEmployer.Id, Role = "Employer" };
        var emailService = Substitute.For<IEmailService>();
        var handler = new UpdateApplicationStatusCommandHandler(db, currentUser, emailService);

        var act = () => handler.Handle(new UpdateApplicationStatusCommand(application.Id, ApplicationStatus.Rejected), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
