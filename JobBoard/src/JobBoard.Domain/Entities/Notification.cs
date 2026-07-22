using JobBoard.Domain.Common;

namespace JobBoard.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string Type { get; set; } = default!; // e.g. "ApplicationReceived", "StatusChanged"
    public string Message { get; set; } = default!;
    public bool IsRead { get; set; }
}
