namespace GymFlow.Dal.Repositories.Implementations;

/// <summary>
/// Repository implementation for User entity with specialized query methods.
/// </summary>
/// <remarks>
/// This class extends the generic repository with specific methods for user management,
/// including eager loading of related entities and domain-specific business queries.
/// </remarks>
public class UserRepository : Repository<User>, IUserRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="dbContextFactory">Factory for creating DbContext instances.</param>
    public UserRepository(IDbContextFactory<AppDbContext> dbContextFactory) 
        : base(dbContextFactory) { }

    /// <inheritdoc />
    public async Task<User?> GetUserWithWorkoutPlansAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.Users
            .Include(u => u.WorkoutPlans)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    /// <inheritdoc />
    public async Task<User?> GetUserWithProgressLogsAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.Users
            .Include(u => u.ProgressLogs)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    /// <inheritdoc />
    public async Task<User?> GetUserWithCompleteHistoryAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.Users
            .Include(u => u.WorkoutPlans)
                .ThenInclude(wp => wp.WorkoutDays)
                    .ThenInclude(wd => wd.WorkoutDayExercises)
                        .ThenInclude(wde => wde.Exercise)
            .Include(u => u.ProgressLogs)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await using var context = await CreateContextAsync();
        return await context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        await using var context = await CreateContextAsync();
        return await context.Users
            .Where(u => u.WorkoutPlans.Any(wp => wp.IsActive))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetUsersByBodyTypeAsync(string bodyType)
    {
        await using var context = await CreateContextAsync();
        return await context.Users
            .Where(u => u.BodyType != null && u.BodyType.ToString() == bodyType)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> IsUserActiveInWorkoutAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutPlans
            .AnyAsync(wp => wp.UserId == userId && wp.IsActive);
    }

    /// <inheritdoc />
    public async Task<int> GetTotalWorkoutCountAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutSessions
            .Include(ws => ws.WorkoutDay)
                .ThenInclude(wd => wd!.WorkoutPlan)
            .Where(ws => ws.WorkoutDay != null && ws.WorkoutDay.WorkoutPlan!.UserId == userId)
            .CountAsync();
    }
}