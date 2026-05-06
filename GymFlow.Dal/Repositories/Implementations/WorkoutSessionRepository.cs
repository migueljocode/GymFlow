namespace GymFlow.Dal.Repositories.Implementations;

/// <summary>
/// Repository implementation for WorkoutSession entity with specialized query methods.
/// </summary>
public class WorkoutSessionRepository : Repository<WorkoutSession>, IWorkoutSessionRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkoutSessionRepository"/> class.
    /// </summary>
    /// <param name="dbContextFactory">Factory for creating DbContext instances.</param>
    public WorkoutSessionRepository(IDbContextFactory<AppDbContext> dbContextFactory) 
        : base(dbContextFactory) { }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkoutSession>> GetSessionsByUserAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutSessions
            .Include(ws => ws.WorkoutDay)
                .ThenInclude(wd => wd!.WorkoutPlan)
            .Where(ws => ws.WorkoutDay != null && ws.WorkoutDay.WorkoutPlan!.UserId == userId)
            .OrderByDescending(ws => ws.ActualDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkoutSession>> GetSessionsByWorkoutDayAsync(int workoutDayId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutSessions
            .Where(ws => ws.WorkoutDayId == workoutDayId)
            .OrderByDescending(ws => ws.ActualDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkoutSession>> GetSessionsByDateRangeAsync(int userId, DateOnly startDate, DateOnly endDate)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutSessions
            .Include(ws => ws.WorkoutDay)
                .ThenInclude(wd => wd!.WorkoutPlan)
            .Where(ws => ws.WorkoutDay != null && 
                         ws.WorkoutDay.WorkoutPlan!.UserId == userId &&
                         ws.ActualDate >= startDate && 
                         ws.ActualDate <= endDate)
            .OrderBy(ws => ws.ActualDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<WorkoutSession?> GetLatestSessionAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutSessions
            .Include(ws => ws.WorkoutDay)
            .Where(ws => ws.WorkoutDay != null && ws.WorkoutDay.WorkoutPlan!.UserId == userId)
            .OrderByDescending(ws => ws.ActualDate)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int> GetSessionCountByUserAsync(int userId, DateOnly? fromDate = null)
    {
        await using var context = await CreateContextAsync();
        var query = context.WorkoutSessions
            .Include(ws => ws.WorkoutDay)
            .Where(ws => ws.WorkoutDay != null && ws.WorkoutDay.WorkoutPlan!.UserId == userId);
        
        if (fromDate.HasValue)
        {
            query = query.Where(ws => ws.ActualDate >= fromDate.Value);
        }
        
        return await query.CountAsync();
    }

    /// <inheritdoc />
    public async Task<double> GetAverageSessionDurationAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        var avg = await context.WorkoutSessions
            .Include(ws => ws.WorkoutDay)
            .Where(ws => ws.WorkoutDay != null && ws.WorkoutDay.WorkoutPlan!.UserId == userId)
            .AverageAsync(ws => ws.ActualDurationMinutes);
        
        return avg;
    }

    /// <inheritdoc />
    public async Task<bool> HasUserCompletedWorkoutDayAsync(int workoutDayId, DateOnly date)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutSessions
            .AnyAsync(ws => ws.WorkoutDayId == workoutDayId && ws.ActualDate == date);
    }
}