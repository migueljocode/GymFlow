namespace GymFlow.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public AuthService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        
        var person = await context.Persons
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Username == username);
        
        if (person == null)
        {
            Console.WriteLine($"[Auth] User not found: {username}");
            return null;
        }
        
        Console.WriteLine($"[Auth] User found: {person.Username}, Password in DB: '{person.Password}', Input password: '{password}'");
        
        if (person.Password != password)
        {
            Console.WriteLine($"[Auth] Password mismatch for user: {username}");
            return null;
        }
        
        Console.WriteLine($"[Auth] Authentication successful for: {username}");
        return person.User;
    }
}