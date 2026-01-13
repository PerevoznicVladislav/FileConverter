using FileConverter.Data.Models;
using FileConverter.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileConverter.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly FileConverterDbContext _context;

    public UserRepository(FileConverterDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.UserPlan)
                .ThenInclude(up => up!.Plan)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserPlan)
                .ThenInclude(up => up!.Plan)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> UserExistsAsync(Guid id)
    {
        return await _context.Users.AnyAsync(u => u.UserId == id);
    }
}
