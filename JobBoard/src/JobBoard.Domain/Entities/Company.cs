using JobBoard.Domain.Common;

namespace JobBoard.Domain.Entities;

public class Company : BaseEntity
{
    public Guid OwnerUserId { get; set; }
    public User OwnerUser { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }

    public ICollection<JobListing> JobListings { get; set; } = new List<JobListing>();
}
