using JobBoard.Domain.Common;
using JobBoard.Domain.Enums;

namespace JobBoard.Domain.Entities;

public class JobListing : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Location { get; set; } = default!;
    public bool IsRemote { get; set; }

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public JobType JobType { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Draft;

    public string? Tags { get; set; } // comma-separated for v1

    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ViewCount { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
