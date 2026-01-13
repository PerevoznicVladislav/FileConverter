using FileConverter.BLL.DTOs.Users;

namespace FileConverter.BLL.Services.Interfaces;

public interface IUserService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
    Task<LoginResponseDto> RegisterAsync(RegisterDto registerDto);
}
