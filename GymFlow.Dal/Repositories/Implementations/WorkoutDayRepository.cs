namespace GymFlow.Dal.Repositories.Implementations;

/// <summary>
/// Repository implementation for WorkoutDay entity with specialized query methods.
/// </summary>
public class WorkoutDayRepository : Repository<WorkoutDay>, IWorkoutDayRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkoutDayRepository"/> class.
    /// </summary>
    /// <param name="dbContextFactory">Factory for creating DbContext instances.</param>
    public WorkoutDayRepository(IDbContextFactory<AppDbContext> dbContextFactory) 
        : base(dbContextFactory) { }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkoutDay>> GetWorkoutDaysByPlanAsync(int planId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutDays
            .Where(wd => wd.WorkoutPlanId == planId)
            .OrderBy(wd => wd.DayOfWeek)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<WorkoutDay?> GetWorkoutDayWithExercisesAsync(int workoutDayId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutDays
            .Include(wd => wd.WorkoutDayExercises)
                .ThenInclude(wde => wde.Exercise)
            .FirstOrDefaultAsync(wd => wd.Id == workoutDayId);
    }

    /// <inheritdoc />
    public async Task<WorkoutDay?> GetWorkoutDayWithSessionsAsync(int workoutDayId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutDays
            .Include(wd => wd.WorkoutSessions)
            .FirstOrDefaultAsync(wd => wd.Id == workoutDayId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkoutDay>> GetWorkoutDaysByWeekdayAsync(DayOfWeek dayOfWeek)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutDays
            .Where(wd => wd.DayOfWeek == dayOfWeek)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<WorkoutDay?> GetWorkoutDayByWeekdayAndPlanAsync(int planId, DayOfWeek dayOfWeek)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutDays
            .FirstOrDefaultAsync(wd => wd.WorkoutPlanId == planId && wd.DayOfWeek == dayOfWeek);
    }

    /// <inheritdoc />
    public async Task<int> GetTotalExercisesCountAsync(int workoutDayId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutDayExercises
            .Where(wde => wde.WorkoutDayId == workoutDayId)
            .CountAsync();
    }
}