namespace FileConverter.BLL.DTOs.Users;

public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
}
