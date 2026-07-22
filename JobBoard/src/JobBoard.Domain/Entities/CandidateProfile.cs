using JobBoard.Domain.Common;

namespace JobBoard.Domain.Entities;

public class CandidateProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string FullName { get; set; } = default!;
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public string? ResumeUrl { get; set; }
    public string? Skills { get; set; } // comma-separated for v1; normalize to a Skill table later

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
