namespace GymFlow.Dal.Repositories.Implementations;

/// <summary>
/// Repository implementation for ProgressLog entity with specialized query methods.
/// </summary>
public class ProgressLogRepository : Repository<ProgressLog>, IProgressLogRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressLogRepository"/> class.
    /// </summary>
    /// <param name="dbContextFactory">Factory for creating DbContext instances.</param>
    public ProgressLogRepository(IDbContextFactory<AppDbContext> dbContextFactory) 
        : base(dbContextFactory) { }

    /// <inheritdoc />
    public async Task<IEnumerable<ProgressLog>> GetUserProgressHistoryAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.ProgressLogs
            .Where(pl => pl.UserId == userId)
            .OrderByDescending(pl => pl.LogDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProgressLog>> GetUserProgressByPlanAsync(int userId, int? planId = null)
    {
        await using var context = await CreateContextAsync();
        var query = context.ProgressLogs.Where(pl => pl.UserId == userId);
        
        if (planId.HasValue)
            query = query.Where(pl => pl.WorkoutPlanId == planId);
        else
            query = query.Where(pl => pl.WorkoutPlanId == null);
        
        return await query.OrderBy(pl => pl.LogDate).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<ProgressLog?> GetLatestProgressLogAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        return await context.ProgressLogs
            .Where(pl => pl.UserId == userId)
            .OrderByDescending(pl => pl.LogDate)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<ProgressLog?> GetProgressLogByDateAsync(int userId, DateOnly date)
    {
        await using var context = await CreateContextAsync();
        return await context.ProgressLogs
            .FirstOrDefaultAsync(pl => pl.UserId == userId && pl.LogDate == date);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProgressLog>> GetWeightTrendAsync(int userId, int lastNEntries)
    {
        await using var context = await CreateContextAsync();
        return await context.ProgressLogs
            .Where(pl => pl.UserId == userId)
            .OrderByDescending(pl => pl.LogDate)
            .Take(lastNEntries)
            .OrderBy(pl => pl.LogDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<float?> GetAverageWeeklyProgressAsync(int userId, int weeks = 4)
    {
        await using var context = await CreateContextAsync();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-weeks * 7));
        
        var logs = await context.ProgressLogs
            .Where(pl => pl.UserId == userId && pl.LogDate >= startDate)
            .OrderBy(pl => pl.LogDate)
            .ToListAsync();

        if (logs.Count < 2)
            return null;

        var firstWeight = logs.First().Weight;
        var lastWeight = logs.Last().Weight;
        var weeksSpan = (float)(logs.Last().LogDate.DayNumber - logs.First().LogDate.DayNumber) / 7;

        return weeksSpan > 0 ? (lastWeight - firstWeight) / weeksSpan : null;
    }

    /// <inheritdoc />
    public async Task<float?> GetWeightDifferenceAsync(int userId, DateOnly fromDate, DateOnly toDate)
    {
        await using var context = await CreateContextAsync();
        var fromLog = await context.ProgressLogs
            .FirstOrDefaultAsync(pl => pl.UserId == userId && pl.LogDate == fromDate);
        var toLog = await context.ProgressLogs
            .FirstOrDefaultAsync(pl => pl.UserId == userId && pl.LogDate == toDate);
        
        if (fromLog is null || toLog is null)
            return null;
        
        return toLog.Weight - fromLog.Weight;
    }
}