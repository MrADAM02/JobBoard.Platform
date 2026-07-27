using FluentAssertions;
using JobBoard.Application.Features.Companies.Queries.GetEmployerAnalytics;
using JobBoard.Application.UnitTests.TestHelpers;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using JobBoard.Infrastructure.Persistence;

namespace JobBoard.Application.UnitTests.Features.Companies;

public class GetEmployerAnalyticsQueryHandlerTests
{
    [Fact]
    public async Task Employer_with_no_company_gets_null()
    {
        var db = TestDbContextFactory.Create();
        var employer = new User { Email = "employer@test.com", PasswordHash = "x", Role = UserRole.Employer };
        db.Users.Add(employer);
        db.SaveChanges();

        var currentUser = new FakeCurrentUserService { UserId = employer.Id, Role = "Employer" };
        var handler = new GetEmployerAnalyticsQueryHandler(db, currentUser);

        var result = await handler.Handle(new GetEmployerAnalyticsQuery(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Views_by_day_is_zero_filled_across_30_days()
    {
        var db = TestDbContextFactory.Create();
        var employer = new User { Email = "employer@test.com", PasswordHash = "x", Role = UserRole.Employer };
        var company = new Company { OwnerUser = employer, OwnerUserId = employer.Id, Name = "Acme" };
        var job = new JobListing
        {
            Company = company, CompanyId = company.Id, Title = "Backend Engineer",
            Description = "Build APIs", Location = "Remote", JobType = JobType.FullTime,
            Status = JobStatus.Published, ViewCount = 2
        };

        db.Users.Add(employer);
        db.Companies.Add(company);
        db.JobListings.Add(job);
        // Only "today" gets a real view - every other of the 30 days should zero-fill.
        db.JobViews.Add(new JobView { JobListingId = job.Id, ViewedAt = DateTime.UtcNow });
        db.JobViews.Add(new JobView { JobListingId = job.Id, ViewedAt = DateTime.UtcNow });
        db.SaveChanges();

        var currentUser = new FakeCurrentUserService { UserId = employer.Id, Role = "Employer" };
        var handler = new GetEmployerAnalyticsQueryHandler(db, currentUser);

        var result = await handler.Handle(new GetEmployerAnalyticsQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ViewsByDay.Should().HaveCount(30);
        result.ViewsByDay.Sum(d => d.Count).Should().Be(2);
        result.ViewsByDay.Last().Count.Should().Be(2); // today is the last entry in the 30-day window
        result.TotalViews.Should().Be(2);
        result.TotalJobsPosted.Should().Be(1);
        result.TopJobs.Should().ContainSingle(j => j.Id == job.Id && j.ViewCount == 2);
    }
}
