using FluentAssertions;
using JobBoard.Application.Features.Jobs.Queries.GetSimilarJobs;
using JobBoard.Application.UnitTests.TestHelpers;
using JobBoard.Domain.Entities;
using JobBoard.Domain.Enums;
using JobBoard.Infrastructure.Persistence;

namespace JobBoard.Application.UnitTests.Features.Jobs;

public class GetSimilarJobsQueryHandlerTests
{
    private static async Task<(ApplicationDbContext Db, JobListing Target)> SeedAsync()
    {
        var db = TestDbContextFactory.Create();

        var company = new Company { OwnerUserId = Guid.NewGuid(), Name = "Acme" };
        db.Companies.Add(company);

        var target = new JobListing
        {
            CompanyId = company.Id, Company = company, Title = "Backend Engineer",
            Description = "Build APIs in C#", Location = "Remote", IsRemote = true,
            SalaryMin = 90000, SalaryMax = 130000, JobType = JobType.FullTime,
            Status = JobStatus.Published, PublishedAt = DateTime.UtcNow.AddDays(-1)
        };

        db.JobListings.AddRange(
            target,
            new JobListing
            {
                CompanyId = company.Id, Company = company, Title = "Both Match",
                Description = "Same type and location", Location = "Remote", IsRemote = true,
                SalaryMin = 80000, SalaryMax = 120000, JobType = JobType.FullTime,
                Status = JobStatus.Published, PublishedAt = DateTime.UtcNow.AddDays(-2)
            },
            new JobListing
            {
                CompanyId = company.Id, Company = company, Title = "Type Only Match",
                Description = "Same type, different location", Location = "New York", IsRemote = false,
                SalaryMin = 80000, SalaryMax = 120000, JobType = JobType.FullTime,
                Status = JobStatus.Published, PublishedAt = DateTime.UtcNow
            },
            new JobListing
            {
                CompanyId = company.Id, Company = company, Title = "No Match",
                Description = "Different type and location", Location = "New York", IsRemote = false,
                SalaryMin = 50000, SalaryMax = 70000, JobType = JobType.Contract,
                Status = JobStatus.Published, PublishedAt = DateTime.UtcNow
            },
            new JobListing
            {
                CompanyId = company.Id, Company = company, Title = "Unpublished Match",
                Description = "Same type and location but a draft", Location = "Remote", IsRemote = true,
                JobType = JobType.FullTime, Status = JobStatus.Draft
            });

        await db.SaveChangesAsync();
        return (db, target);
    }

    [Fact]
    public async Task Ranks_listings_matching_both_type_and_location_first()
    {
        var (db, target) = await SeedAsync();
        var handler = new GetSimilarJobsQueryHandler(db);

        var result = await handler.Handle(new GetSimilarJobsQuery(target.Id), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Both Match");
        result[1].Title.Should().Be("Type Only Match");
    }

    [Fact]
    public async Task Excludes_the_target_listing_unpublished_listings_and_non_matching_listings()
    {
        var (db, target) = await SeedAsync();
        var handler = new GetSimilarJobsQueryHandler(db);

        var result = await handler.Handle(new GetSimilarJobsQuery(target.Id), CancellationToken.None);

        result.Should().NotContain(j => j.Title == target.Title);
        result.Should().NotContain(j => j.Title == "No Match");
        result.Should().NotContain(j => j.Title == "Unpublished Match");
    }

    [Fact]
    public async Task Returns_an_empty_list_when_the_job_does_not_exist()
    {
        var (db, _) = await SeedAsync();
        var handler = new GetSimilarJobsQueryHandler(db);

        var result = await handler.Handle(new GetSimilarJobsQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
