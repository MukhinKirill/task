using Microsoft.EntityFrameworkCore;
using Task.Connector.Models;

namespace Task.Connector.Data;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    
    public UserRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async System.Threading.Tasks.Task AddUserAsync(User? user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u != null && u.UserId == userId); 
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u != null && u.Email == email);
    }

    public void AddUser(User user)
    {
        throw new NotImplementedException();
    }
}