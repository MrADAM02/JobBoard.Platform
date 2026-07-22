using JobBoard.Application.Common.Interfaces;

namespace JobBoard.Application.UnitTests.TestHelpers;

public class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}
