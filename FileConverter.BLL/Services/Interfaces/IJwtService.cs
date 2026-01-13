using System.Security.Claims;

namespace FileConverter.BLL.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email);
    ClaimsPrincipal? ValidateToken(string token);
}
