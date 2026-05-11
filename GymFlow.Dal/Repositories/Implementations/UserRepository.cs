namespace GymFlow.Dal.Repositories.Implementations;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(IDbContextFactory<AppDbContext> dbContextFactory) 
        : base(dbContextFactory) { }

    public async Task<User?> GetUserWithPersonAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserWithWorkoutPlansAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.Users
            .Include(u => u.Person)
            .Include(u => u.WorkoutPlans)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserWithProgressLogsAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.Users
            .Include(u => u.Person)
            .Include(u => u.ProgressLogs)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByPersonIdAsync(int personId)
    {
        await using var context = await CreateContextAsync();
        return await context.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.PersonId == personId);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        await using var context = await CreateContextAsync();
        return await context.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Person != null && u.Person.Username == username);
    }

    public async Task<IEnumerable<User>> GetAllUsersWithPersonAsync()
    {
        await using var context = await CreateContextAsync();
        return await context.Users
            .Include(u => u.Person)
            .ToListAsync();
    }
    public async Task<User?> GetUserWithCompleteHistoryAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.Users
            .Include(u => u.Person)
            .Include(u => u.WorkoutPlans)
                .ThenInclude(wp => wp.WorkoutDays)
            .Include(u => u.ProgressLogs)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await using var context = await CreateContextAsync();
        return await context.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Person != null && u.Person.Email == email);
    }
}