using FluentAssertions;
using JobBoard.Application.Features.Jobs.Queries.GetJobListings;
using JobBoard.Application.UnitTests.TestHelpers;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using JobBoard.Infrastructure.Persistence;

namespace JobBoard.Application.UnitTests.Features.Jobs;

public class GetJobListingsQueryHandlerTests
{
    private static async Task<ApplicationDbContext> SeedAsync()
    {
        var db = TestDbContextFactory.Create();

        var company = new Company { OwnerUserId = Guid.NewGuid(), Name = "Acme" };
        db.Companies.Add(company);

        db.JobListings.AddRange(
            new JobListing
            {
                CompanyId = company.Id, Company = company, Title = "Backend Engineer",
                Description = "Build APIs in C#", Location = "Remote", IsRemote = true,
                SalaryMin = 90000, SalaryMax = 130000, JobType = JobType.FullTime,
                Status = JobStatus.Published, PublishedAt = DateTime.UtcNow
            },
            new JobListing
            {
                CompanyId = company.Id, Company = company, Title = "Frontend Engineer",
                Description = "Build UIs in Vue", Location = "New York", IsRemote = false,
                SalaryMin = 70000, SalaryMax = 100000, JobType = JobType.FullTime,
                Status = JobStatus.Published, PublishedAt = DateTime.UtcNow
            },
            new JobListing
            {
                CompanyId = company.Id, Company = company, Title = "Contract QA",
                Description = "Manual and automated testing", Location = "Remote", IsRemote = true,
                SalaryMin = 40000, SalaryMax = 60000, JobType = JobType.Contract,
                Status = JobStatus.Published, PublishedAt = DateTime.UtcNow
            },
            new JobListing
            {
                CompanyId = company.Id, Company = company, Title = "Draft Role",
                Description = "Not visible yet", Location = "Remote", IsRemote = true,
                JobType = JobType.FullTime, Status = JobStatus.Draft
            });

        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task Only_published_listings_are_returned()
    {
        var db = await SeedAsync();
        var handler = new GetJobListingsQueryHandler(db);

        var result = await handler.Handle(new GetJobListingsQuery(null, null, null, null, null), CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.Items.Should().NotContain(j => j.Title == "Draft Role");
    }

    [Fact]
    public async Task Keyword_filters_by_title_or_description()
    {
        var db = await SeedAsync();
        var handler = new GetJobListingsQueryHandler(db);

        var result = await handler.Handle(new GetJobListingsQuery("Vue", null, null, null, null), CancellationToken.None);

        result.Items.Should().ContainSingle(j => j.Title == "Frontend Engineer");
    }

    [Fact]
    public async Task Location_filter_matches_substring()
    {
        var db = await SeedAsync();
        var handler = new GetJobListingsQueryHandler(db);

        var result = await handler.Handle(new GetJobListingsQuery(null, "New York", null, null, null), CancellationToken.None);

        result.Items.Should().ContainSingle(j => j.Title == "Frontend Engineer");
    }

    [Fact]
    public async Task JobType_filter_restricts_to_matching_type()
    {
        var db = await SeedAsync();
        var handler = new GetJobListingsQueryHandler(db);

        var result = await handler.Handle(new GetJobListingsQuery(null, null, JobType.Contract, null, null), CancellationToken.None);

        result.Items.Should().ContainSingle(j => j.Title == "Contract QA");
    }

    [Fact]
    public async Task RemoteOnly_filter_excludes_non_remote_listings()
    {
        var db = await SeedAsync();
        var handler = new GetJobListingsQueryHandler(db);

        var result = await handler.Handle(new GetJobListingsQuery(null, null, null, true, null), CancellationToken.None);

        result.Items.Should().OnlyContain(j => j.IsRemote);
        result.Items.Should().NotContain(j => j.Title == "Frontend Engineer");
    }

    [Fact]
    public async Task MinSalary_filter_excludes_listings_whose_max_is_below_threshold()
    {
        var db = await SeedAsync();
        var handler = new GetJobListingsQueryHandler(db);

        var result = await handler.Handle(new GetJobListingsQuery(null, null, null, null, 80000), CancellationToken.None);

        result.Items.Should().NotContain(j => j.Title == "Contract QA");
        result.Items.Should().Contain(j => j.Title == "Backend Engineer");
        result.Items.Should().Contain(j => j.Title == "Frontend Engineer");
    }
}
