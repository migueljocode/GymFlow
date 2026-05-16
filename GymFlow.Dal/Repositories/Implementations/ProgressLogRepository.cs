using GymFlow.Models.Exceptions;

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

        // حالت 1: کمتر از 2 لاگ در بازه زمانی مورد نظر
        if (logs.Count < 2)
        {
            // بررسی اینکه آیا اصلاً هیچ لاگی برای این کاربر وجود دارد یا نه
            var anyLog = await context.ProgressLogs
                .AnyAsync(pl => pl.UserId == userId);
            
            // حالت 1.1: هیچ لاگی وجود ندارد
            if (!anyLog)
            {
                throw new InsufficientDataException(
                    $"No progress logs found for user ID {userId}. " +
                    "Please log at least 2 weight entries before analyzing trends.");
            }
            
            // گرفتن قدیمی‌ترین و جدیدترین لاگ برای بررسی بازه زمانی
            var oldestLog = await context.ProgressLogs
                .Where(pl => pl.UserId == userId)
                .OrderBy(pl => pl.LogDate)
                .FirstOrDefaultAsync();
            
            var latestLog = await context.ProgressLogs
                .Where(pl => pl.UserId == userId)
                .OrderByDescending(pl => pl.LogDate)
                .FirstOrDefaultAsync();
            
            // حالت 1.2: لاگ وجود دارد ولی جدیدترین لاگ هم از startDate قدیمی‌تر است
            // یعنی تمام لاگ‌ها خارج از بازه زمانی مورد نظر هستند
            if (oldestLog != null && latestLog != null && latestLog.LogDate < startDate)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var actualWeeks = (today.DayNumber - oldestLog.LogDate.DayNumber) / 7;
                
                throw new DataOutOfRangeException(
                    $"Not enough recent data for analysis. " +
                    $"Required: {weeks} weeks of data, " +
                    $"Available: {actualWeeks} weeks of data (from {oldestLog.LogDate} to {today}). " +
                    $"Please log weight at least once every week.",
                    requiredWeeks: weeks,
                    availableWeeks: actualWeeks,
                    earliestDate: oldestLog.LogDate.ToDateTime(TimeOnly.MinValue),
                    latestDate: today.ToDateTime(TimeOnly.MinValue));
            }
            
            // حالت 1.3: لاگ در بازه وجود دارد ولی تعدادش کمتر از 2 است
            throw new InsufficientDataException(
                $"Insufficient data for user ID {userId}. " +
                $"Found {logs.Count} log(s) in the last {weeks} weeks. " +
                $"Need at least 2 logs to calculate weekly average. " +
                "Please log your weight more frequently.");
        }

        // حالت 2: حداقل 2 لاگ در بازه زمانی داریم
        var firstWeight = logs.First().Weight;
        var lastWeight = logs.Last().Weight;
        var weeksSpan = (float)(logs.Last().LogDate.DayNumber - logs.First().LogDate.DayNumber) / 7;

        // حالت 2.1: فاصله زمانی بین اولین و آخرین لاگ کمتر از 0.5 هفته (3.5 روز) است
        if (weeksSpan < 0.5f)
        {
            var daysDiff = logs.Last().LogDate.DayNumber - logs.First().LogDate.DayNumber;
            throw new InsufficientDataException(
                $"Insufficient time span for analysis. " +
                $"Only {daysDiff} day(s) between first and last log. " +
                $"Please log your weight at least one week apart for accurate trend analysis.");
        }

        // حالت 2.2: محاسبه موفق - بازگشت میانگین تغییر وزنی در هفته
        var weeklyChange = (lastWeight - firstWeight) / weeksSpan;
        
        // اگر weeklyChange خیلی بزرگ یا خیلی کوچک باشد (بیشتر از 5 کیلو در هفته)،
        // احتمالاً داده‌ها غیرواقعی هستند
        if (Math.Abs(weeklyChange) > 5f)
        {
            throw new InsufficientDataException(
                $"Unrealistic weight change detected: {weeklyChange:F1} kg per week. " +
                "Please check your weight entries for accuracy. " +
                "Healthy weight change is typically between -1 and 1 kg per week.");
        }
        
        return weeklyChange;
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