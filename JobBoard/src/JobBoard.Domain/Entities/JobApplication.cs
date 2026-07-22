using JobBoard.Domain.Common;
using JobBoard.Domain.Enums;

namespace JobBoard.Domain.Entities;

// Named "JobApplication" (not "Application") to avoid clashing with
// Microsoft.AspNetCore's own Application/IApplicationBuilder types.
public class JobApplication : BaseEntity
{
    public Guid JobListingId { get; set; }
    public JobListing JobListing { get; set; } = default!;

    public Guid CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = default!;

    public string? CoverLetter { get; set; }
    public string ResumeUrlSnapshot { get; set; } = default!; // resume as of application time
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}
