using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using JobBoard.Application.Features.Auth.Commands.Register;
using JobBoard.Application.Features.Companies.Commands.CreateCompany;
using JobBoard.Application.Features.Jobs.Commands.CreateJobListing;
using JobBoard.Domain.Enums;

namespace JobBoard.Api.IntegrationTests;

// Covers the core "register -> login -> create job -> apply" journey over real HTTP,
// exercising routing, auth middleware, and the MediatR pipeline together - not just
// individual handlers in isolation (that's what the Application unit tests are for).
public class CoreJourneyTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CoreJourneyTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Employer_can_register_post_a_job_and_a_candidate_can_apply()
    {
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];

        // Register employer
        var employerRegister = await _client.PostAsJsonAsync("/api/auth/register", new RegisterCommand(
            $"employer{uniqueSuffix}@test.com", "Password123!", "Emp Owner", UserRole.Employer));
        employerRegister.StatusCode.Should().Be(HttpStatusCode.OK);
        var employerAuth = await employerRegister.Content.ReadFromJsonAsync<AuthResultDto>();
        employerAuth.Should().NotBeNull();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", employerAuth!.AccessToken);

        // Create company
        var companyResponse = await _client.PostAsJsonAsync("/api/companies", new CreateCompanyCommand(
            "Acme Corp", "https://acme.example", "Widgets", "Remote"));
        companyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var companyId = await companyResponse.Content.ReadFromJsonAsync<Guid>();

        // Post a job, published immediately
        var jobResponse = await _client.PostAsJsonAsync("/api/jobs", new CreateJobListingCommand(
            companyId, "Backend Engineer", "Build APIs", "Remote", true, 90000, 130000,
            JobType.FullTime, "csharp,dotnet", PublishImmediately: true));
        jobResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var jobId = await jobResponse.Content.ReadFromJsonAsync<Guid>();

        // Public listing includes the new job with no auth header
        _client.DefaultRequestHeaders.Authorization = null;
        var publicListing = await _client.GetAsync("/api/jobs?keyword=Backend");
        publicListing.StatusCode.Should().Be(HttpStatusCode.OK);
        var listingBody = await publicListing.Content.ReadAsStringAsync();
        listingBody.Should().Contain(jobId.ToString());

        // Register candidate and apply
        var candidateRegister = await _client.PostAsJsonAsync("/api/auth/register", new RegisterCommand(
            $"candidate{uniqueSuffix}@test.com", "Password123!", "Cand Applicant", UserRole.Candidate));
        candidateRegister.StatusCode.Should().Be(HttpStatusCode.OK);
        var candidateAuth = await candidateRegister.Content.ReadFromJsonAsync<AuthResultDto>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", candidateAuth!.AccessToken);

        var applyResponse = await _client.PostAsJsonAsync("/api/applications", new
        {
            JobListingId = jobId,
            CoverLetter = "I would love this role"
        });
        applyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Applying a second time is rejected
        var secondApply = await _client.PostAsJsonAsync("/api/applications", new
        {
            JobListingId = jobId,
            CoverLetter = "Trying again"
        });
        secondApply.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Creating_a_job_without_a_token_is_unauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/jobs", new CreateJobListingCommand(
            Guid.NewGuid(), "Some Job", "Description", "Remote", true, null, null,
            JobType.FullTime, null, PublishImmediately: false));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Registering_with_a_duplicate_email_returns_bad_request()
    {
        var email = $"dup{Guid.NewGuid():N}@test.com";
        var first = await _client.PostAsJsonAsync("/api/auth/register", new RegisterCommand(
            email, "Password123!", "First User", UserRole.Candidate));
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await _client.PostAsJsonAsync("/api/auth/register", new RegisterCommand(
            email, "Password123!", "Second User", UserRole.Candidate));

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
