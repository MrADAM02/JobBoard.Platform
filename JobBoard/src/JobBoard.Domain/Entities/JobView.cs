using JobBoard.Domain.Common;

namespace JobBoard.Domain.Entities;

// A timestamped log, distinct from JobListing.ViewCount (a running counter
// with no history). Exists purely to back the employer analytics "views
// over time" chart - ViewCount stays the cheap read for everything else.
public class JobView : BaseEntity
{
    public Guid JobListingId { get; set; }
    public JobListing JobListing { get; set; } = default!;

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}
