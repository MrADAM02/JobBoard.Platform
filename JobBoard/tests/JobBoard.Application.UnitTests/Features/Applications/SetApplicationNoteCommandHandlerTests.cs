using FluentAssertions;
using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Features.Applications.Commands.SetApplicationNote;
using JobBoard.Application.UnitTests.TestHelpers;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using JobBoard.Infrastructure.Persistence;

namespace JobBoard.Application.UnitTests.Features.Applications;

public class SetApplicationNoteCommandHandlerTests
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
    public async Task Owning_employer_can_set_note()
    {
        var (db, application, owningEmployer, _) = Seed();
        var currentUser = new FakeCurrentUserService { UserId = owningEmployer.Id, Role = "Employer" };
        var handler = new SetApplicationNoteCommandHandler(db, currentUser);

        await handler.Handle(new SetApplicationNoteCommand(application.Id, "Strong candidate, follow up Monday."), CancellationToken.None);

        db.JobApplications.First(a => a.Id == application.Id).EmployerNotes.Should().Be("Strong candidate, follow up Monday.");
    }

    [Fact]
    public async Task Non_owning_employer_is_forbidden()
    {
        var (db, application, _, otherEmployer) = Seed();
        var currentUser = new FakeCurrentUserService { UserId = otherEmployer.Id, Role = "Employer" };
        var handler = new SetApplicationNoteCommandHandler(db, currentUser);

        var act = () => handler.Handle(new SetApplicationNoteCommand(application.Id, "Snooping"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
        db.JobApplications.First(a => a.Id == application.Id).EmployerNotes.Should().BeNull();
    }
}
