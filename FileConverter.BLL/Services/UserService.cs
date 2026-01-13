using System.Security.Cryptography;
using System.Text;
using FileConverter.BLL.DTOs.Users;
using FileConverter.BLL.Services.Interfaces;
using FileConverter.Data.Models;
using FileConverter.Data.Repositories.Interfaces;

namespace FileConverter.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public UserService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null)
            return null;

        if (!VerifyPassword(loginDto.Password, user.PasswordHash))
            return null;

        var token = _jwtService.GenerateToken(user.UserId, user.Email);

        return new LoginResponseDto
        {
            Token = token,
            UserId = user.UserId,
            Email = user.Email
        };
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        if (await _userRepository.ExistsByEmailAsync(registerDto.Email))
            throw new InvalidOperationException("Email already exists");

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = registerDto.Email,
            PasswordHash = HashPassword(registerDto.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        var token = _jwtService.GenerateToken(user.UserId, user.Email);

        return new LoginResponseDto
        {
            Token = token,
            UserId = user.UserId,
            Email = user.Email
        };
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == passwordHash;
    }
}
