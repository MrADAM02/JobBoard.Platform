using JobBoard.Domain.Common;
using JobBoard.Domain.Enums;

namespace JobBoard.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    // Refresh token (kept simple: one active token per user; swap for a table if you need multi-device support)
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public CandidateProfile? CandidateProfile { get; set; }
    public Company? Company { get; set; }
}
