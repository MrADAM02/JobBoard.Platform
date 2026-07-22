using JobBoard.Domain.Entities;

namespace JobBoard.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
