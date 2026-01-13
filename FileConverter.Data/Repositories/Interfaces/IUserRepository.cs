using FileConverter.Data.Models;

namespace FileConverter.Data.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> AddAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> UserExistsAsync(Guid id);
}
