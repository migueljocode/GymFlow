namespace GymFlow.Dal.Repositories.Implementations;

/// <summary>
/// Repository implementation for WorkoutPlan entity with specialized query methods.
/// </summary>
/// <remarks>
/// Provides concrete implementations for workout plan management, including detailed plan loading,
/// activation/deactivation logic, and phase-based queries.
/// </remarks>
public class WorkoutPlanRepository : Repository<WorkoutPlan>, IWorkoutPlanRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkoutPlanRepository"/> class.
    /// </summary>
    /// <param name="dbContextFactory">Factory for creating DbContext instances.</param>
    public WorkoutPlanRepository(IDbContextFactory<AppDbContext> dbContextFactory) 
        : base(dbContextFactory) { }

    /// <inheritdoc />
    public async Task<WorkoutPlan?> GetWorkoutPlanWithDetailsAsync(int planId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutPlans
            .Include(wp => wp.WorkoutDays)
                .ThenInclude(wd => wd.WorkoutDayExercises)
                    .ThenInclude(wde => wde.Exercise)
            .Include(wp => wp.ProgressLogs)
            .FirstOrDefaultAsync(wp => wp.Id == planId);
    }

    /// <inheritdoc />
    public async Task<WorkoutPlan?> GetWorkoutPlanWithDaysAsync(int planId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutPlans
            .Include(wp => wp.WorkoutDays)
            .FirstOrDefaultAsync(wp => wp.Id == planId);
    }

    /// <inheritdoc />
    public async Task<WorkoutPlan?> GetWorkoutPlanWithProgressAsync(int planId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutPlans
            .Include(wp => wp.ProgressLogs)
            .FirstOrDefaultAsync(wp => wp.Id == planId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkoutPlan>> GetUserWorkoutPlansAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutPlans
            .Where(wp => wp.UserId == userId)
            .OrderByDescending(wp => wp.StartDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<WorkoutPlan?> GetActiveWorkoutPlanAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutPlans
            .FirstOrDefaultAsync(wp => wp.UserId == userId && wp.IsActive);
    }

    /// <inheritdoc />
    public async Task<WorkoutPlan?> GetCurrentWorkoutPlanAsync(int userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await using var context = await CreateContextAsync();
        return await context.WorkoutPlans
            .FirstOrDefaultAsync(wp => wp.UserId == userId && 
                                      wp.StartDate <= today && 
                                      (wp.EndDate == null || wp.EndDate >= today));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WorkoutPlan>> GetWorkoutPlansByPhaseAsync(int userId, int phase)
    {
        await using var context = await CreateContextAsync();
        return await context.WorkoutPlans
            .Where(wp => wp.UserId == userId && wp.Phase == phase)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> DeactivateAllUserPlansAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        var plans = await context.WorkoutPlans
            .Where(wp => wp.UserId == userId && wp.IsActive)
            .ToListAsync();

        if (plans.Count == 0) return false;

        foreach (var plan in plans)
        {
            plan.IsActive = false;
            plan.UpdatedAt = DateTime.UtcNow;
        }

        return await context.SaveChangesAsync() > 0;
    }

    /// <inheritdoc />
    public async Task<bool> ActivateWorkoutPlanAsync(int planId)
    {
        await using var context = await CreateContextAsync();
        var plan = await context.WorkoutPlans.FindAsync(planId);
        if (plan is null) return false;
        
        plan.IsActive = true;
        plan.UpdatedAt = DateTime.UtcNow;
        
        return await context.SaveChangesAsync() > 0;
    }
}