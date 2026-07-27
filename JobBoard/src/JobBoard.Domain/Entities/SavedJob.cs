using JobBoard.Domain.Common;

namespace JobBoard.Domain.Entities;

// Keyed off User (not CandidateProfile) - saving a job is a plain
// bookmark, not part of the candidate's public-facing profile data.
public class SavedJob : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid JobListingId { get; set; }
    public JobListing JobListing { get; set; } = default!;
}
