using StudentLifeTracker.API.Models;

namespace StudentLifeTracker.API.Services;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user);
    string GenerateRefreshToken();
}

