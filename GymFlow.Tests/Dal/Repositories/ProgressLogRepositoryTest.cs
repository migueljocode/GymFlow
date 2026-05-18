namespace GymFlow.Tests.Dal.Repositories;

public class ProgressLogRepositoryTest : IClassFixture<DbContextFixture>
{
    private readonly DbContextFixture _fixture;
    private static int _uniqueCounter = 0;

    public ProgressLogRepositoryTest(DbContextFixture fixture)
    {
        _fixture = fixture;
    }

    // ========== Helper Methods ==========

    private string GetUniqueSuffix()
    {
        _uniqueCounter++;
        return $"{_uniqueCounter}_{Guid.NewGuid():N}";
    }

    private async Task<User> CreateTestUserAsync()
    {
        var personRepo = new PersonRepository(_fixture.DbContextFactory);
        
        var uniqueSuffix = GetUniqueSuffix();
        var person = new Person
        {
            FirstName = "Test",
            LastName = $"User{uniqueSuffix}",
            Username = $"testuser_{uniqueSuffix}",
            Password = "pass123",
            Email = $"testuser_{uniqueSuffix}@test.com",
            Gender = Gender.Male,
            Age = 25,
            Weight = 75f,
            Height = 175f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow
        };
        
        var addedPerson = await personRepo.AddAsync(person);
        await personRepo.SaveChangesAsync();
        
        var userRepo = new UserRepository(_fixture.DbContextFactory);
        var user = new User
        {
            PersonId = addedPerson.Id,
            Goal = Goal.Fitness,
            EstimatedCaloriesIntake = 2500,
            CreatedAt = DateTime.UtcNow
        };
        
        return await userRepo.AddAsync(user);
    }

    private async Task<WorkoutPlan> CreateTestWorkoutPlanAsync(int userId)
    {
        var planRepo = new WorkoutPlanRepository(_fixture.DbContextFactory);
        var plan = new WorkoutPlan
        {
            UserId = userId,
            Phase = 1,
            SessionsPerWeek = 3,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow
        };
        return await planRepo.AddAsync(plan);
    }

    private async Task<ProgressLog> CreateTestProgressLogAsync(int userId, DateOnly? logDate = null, int? planId = null)
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        var date = logDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        
        var log = new ProgressLog
        {
            UserId = userId,
            WorkoutPlanId = planId,
            LogDate = date,
            Weight = 75.5f,
            BodyFatPercentage = 15.5f,
            Notes = $"Test log for user {userId}",
            CreatedAt = DateTime.UtcNow
        };
        return await repo.AddAsync(log);
    }

    private async Task<ProgressLog> CreateTestProgressLogWithWeightAsync(int userId, DateOnly logDate, int? planId, float weight)
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        
        // بررسی وجود لاگ قبلی برای همین تاریخ و کاربر
        var existing = await repo.GetProgressLogByDateAsync(userId, logDate);
        if (existing != null)
        {
            // اگر لاگ وجود داشت، اون رو حذف فیزیکی می‌کنیم (نه soft delete)
            await using var context = _fixture.CreateContext();
            var existingEntity = await context.ProgressLogs
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == existing.Id);
            if (existingEntity != null)
            {
                context.ProgressLogs.Remove(existingEntity);
                await context.SaveChangesAsync();
            }
        }
        
        var log = new ProgressLog
        {
            UserId = userId,
            WorkoutPlanId = planId,
            LogDate = logDate,
            Weight = weight,
            BodyFatPercentage = 15.5f,
            Notes = $"Test log for user {userId} on {logDate}",
            CreatedAt = DateTime.UtcNow
        };
        return await repo.AddAsync(log);
    }

    // ========== Query Tests ==========

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectProgressLog()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var log = await CreateTestProgressLogAsync(user.Id);

        var fetched = await repo.GetByIdAsync(log.Id);

        Assert.NotNull(fetched);
        Assert.Equal(log.Id, fetched.Id);
        Assert.Equal(log.Weight, fetched.Weight);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var fetched = await repo.GetByIdAsync(99999);

        Assert.Null(fetched);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatchingLog()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 10));
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 15));

        var fetched = await repo.FirstOrDefaultAsync(l => l.LogDate == new DateOnly(2024, 1, 10));

        Assert.NotNull(fetched);
        Assert.Equal(new DateOnly(2024, 1, 10), fetched.LogDate);
    }

    [Fact]
    public async Task SingleOrDefaultAsync_WithSingleMatch_ShouldReturnLog()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var log = await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 20));

        var fetched = await repo.SingleOrDefaultAsync(l => l.Id == log.Id);

        Assert.NotNull(fetched);
        Assert.Equal(log.Id, fetched.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllLogs()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 1));
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 2));

        var all = await repo.GetAllAsync();

        Assert.NotNull(all);
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task FindAsync_ShouldReturnFilteredLogs()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 1), null);
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 2), null);

        var found = await repo.FindAsync(l => l.Weight == 75.5f);

        Assert.Equal(2, found.Count());
    }

    [Fact]
    public async Task FindAsync_ById_ShouldReturnLog()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var log = await CreateTestProgressLogAsync(user.Id);

        var fetched = await repo.FindAsync(log.Id);

        Assert.NotNull(fetched);
        Assert.Equal(log.Id, fetched.Id);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrueWhenExists()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestProgressLogAsync(user.Id);

        var exists = await repo.AnyAsync(l => l.UserId == user.Id);

        Assert.True(exists);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnFalseWhenNotExists()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var exists = await repo.AnyAsync(l => l.UserId == 99999);

        Assert.False(exists);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnTrueWhenAllMatch()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 1));
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 2));

        var allMatch = await repo.AllAsync(l => l.UserId == user.Id);

        Assert.True(allMatch);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnFalseWhenNotAllMatch()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user1 = await CreateTestUserAsync();
        var user2 = await CreateTestUserAsync();
        await CreateTestProgressLogAsync(user1.Id, new DateOnly(2024, 1, 1));
        await CreateTestProgressLogAsync(user2.Id, new DateOnly(2024, 1, 2));

        var allMatch = await repo.AllAsync(l => l.UserId == user1.Id);

        Assert.False(allMatch);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnTotalCount()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 1));
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 2));

        var count = await repo.CountAsync();

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 1));
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 2));

        var count = await repo.CountAsync(l => l.UserId == user.Id);

        Assert.Equal(2, count);
    }

    // ========== Command Tests ==========

    [Fact]
    public async Task AddAsync_ShouldSaveProgressLogToDatabase()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var log = new ProgressLog
        {
            UserId = user.Id,
            LogDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Weight = 80.0f,
            BodyFatPercentage = 18.0f,
            Notes = "Test log",
            CreatedAt = DateTime.UtcNow
        };

        var added = await repo.AddAsync(log);

        var fetched = await repo.GetByIdAsync(added.Id);
        Assert.NotNull(fetched);
        Assert.Equal(80.0f, fetched.Weight);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingProgressLog()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var log = await CreateTestProgressLogAsync(user.Id);
        log.Weight = 85.0f;
        log.Notes = "Updated notes";

        var updated = await repo.UpdateAsync(log);

        var fetched = await repo.GetByIdAsync(updated.Id);
        Assert.Equal(85.0f, fetched?.Weight);
        Assert.Equal("Updated notes", fetched?.Notes);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteProgressLog()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var log = await CreateTestProgressLogAsync(user.Id);

        var deleted = await repo.DeleteAsync(log);

        Assert.True(deleted);
        
        var fetched = await repo.GetByIdAsync(log.Id);
        Assert.Null(fetched);
        
        await using var context = _fixture.CreateContext();
        var allLogs = await context.ProgressLogs.IgnoreQueryFilters().ToListAsync();
        var deletedLog = allLogs.FirstOrDefault(l => l.Id == log.Id);
        Assert.NotNull(deletedLog);
        Assert.True(deletedLog.IsDeleted);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldSoftDeleteProgressLogById()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var log = await CreateTestProgressLogAsync(user.Id);

        var deleted = await repo.DeleteByIdAsync(log.Id);

        Assert.True(deleted);
        var fetched = await repo.GetByIdAsync(log.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithInvalidId_ShouldReturnFalse()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var deleted = await repo.DeleteByIdAsync(99999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleLogs()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var logs = new List<ProgressLog>
        {
            new() { UserId = user.Id, LogDate = new DateOnly(2024, 1, 1), Weight = 70f, CreatedAt = DateTime.UtcNow },
            new() { UserId = user.Id, LogDate = new DateOnly(2024, 1, 2), Weight = 71f, CreatedAt = DateTime.UtcNow }
        };

        var added = await repo.AddRangeAsync(logs);

        Assert.Equal(2, added.Count());
        var all = await repo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldSoftDeleteMultipleLogs()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var logs = new List<ProgressLog>
        {
            new() { UserId = user.Id, LogDate = new DateOnly(2024, 1, 1), Weight = 70f, CreatedAt = DateTime.UtcNow },
            new() { UserId = user.Id, LogDate = new DateOnly(2024, 1, 2), Weight = 71f, CreatedAt = DateTime.UtcNow }
        };
        var added = await repo.AddRangeAsync(logs);

        var deleted = await repo.DeleteRangeAsync(added);

        Assert.True(deleted);
        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task DeleteAllAsync_ShouldSoftDeleteAllLogs()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user1 = await CreateTestUserAsync();
        var user2 = await CreateTestUserAsync();
        
        await CreateTestProgressLogAsync(user1.Id, new DateOnly(2024, 1, 1));
        await CreateTestProgressLogAsync(user1.Id, new DateOnly(2024, 1, 2));
        await CreateTestProgressLogAsync(user2.Id, new DateOnly(2024, 1, 3));

        var deleted = await repo.DeleteAllAsync();

        Assert.True(deleted);
        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    // ========== Specific Interface Tests ==========

    [Fact]
    public async Task GetUserProgressHistoryAsync_ShouldReturnLogsOrderedByDateDescending()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 10));
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 20));
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 15));

        var history = await repo.GetUserProgressHistoryAsync(user.Id);
        var historyList = history.ToList();

        Assert.Equal(3, historyList.Count);
        Assert.True(historyList[0].LogDate >= historyList[1].LogDate);
        Assert.True(historyList[1].LogDate >= historyList[2].LogDate);
    }

    [Fact]
    public async Task GetUserProgressByPlanAsync_ShouldReturnLogsForSpecificPlan()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan1 = await CreateTestWorkoutPlanAsync(user.Id);
        var plan2 = await CreateTestWorkoutPlanAsync(user.Id);
        
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 1), plan1.Id);
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 2), plan1.Id);
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 3), plan2.Id);

        var plan1Logs = await repo.GetUserProgressByPlanAsync(user.Id, plan1.Id);
        var plan2Logs = await repo.GetUserProgressByPlanAsync(user.Id, plan2.Id);

        Assert.Equal(2, plan1Logs.Count());
        Assert.Single(plan2Logs);
    }

    [Fact]
    public async Task GetUserProgressByPlanAsync_WithNullPlan_ShouldReturnLogsWithoutPlan()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 1), null);
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 2), plan.Id);

        var logsWithoutPlan = await repo.GetUserProgressByPlanAsync(user.Id, null);

        Assert.Single(logsWithoutPlan);
        Assert.Null(logsWithoutPlan.First().WorkoutPlanId);
    }

    [Fact]
    public async Task GetLatestProgressLogAsync_ShouldReturnMostRecentLog()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 10));
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 20));
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 15));

        var latest = await repo.GetLatestProgressLogAsync(user.Id);

        Assert.NotNull(latest);
        Assert.Equal(new DateOnly(2024, 1, 20), latest.LogDate);
    }

    [Fact]
    public async Task GetProgressLogByDateAsync_ShouldReturnLogForSpecificDate()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var targetDate = new DateOnly(2024, 1, 15);
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 10));
        await CreateTestProgressLogAsync(user.Id, targetDate);
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 20));

        var log = await repo.GetProgressLogByDateAsync(user.Id, targetDate);

        Assert.NotNull(log);
        Assert.Equal(targetDate, log.LogDate);
    }

    [Fact]
    public async Task GetProgressLogByDateAsync_WithNoLogForDate_ShouldReturnNull()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 10));

        var log = await repo.GetProgressLogByDateAsync(user.Id, new DateOnly(2024, 1, 15));

        Assert.Null(log);
    }

    [Fact]
    public async Task GetWeightTrendAsync_ShouldReturnLastNEntriesOrderedChronologically()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 1));
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 5));
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 10));
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 15));
        await CreateTestProgressLogAsync(user.Id, new DateOnly(2024, 1, 20));

        var trend = await repo.GetWeightTrendAsync(user.Id, 3);
        var trendList = trend.ToList();

        Assert.Equal(3, trendList.Count);
        Assert.True(trendList[0].LogDate <= trendList[1].LogDate);
        Assert.True(trendList[1].LogDate <= trendList[2].LogDate);
    }

    [Fact]
    public async Task GetAverageWeeklyProgressAsync_WithInsufficientData_ShouldThrowInsufficientDataException()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        // فقط یک لاگ در بازه 4 هفته - باید InsufficientDataException بگیریم
        // چون به حداقل 2 لاگ نیاز داریم
        await CreateTestProgressLogWithWeightAsync(user.Id, today, null, 80f);

        var exception = await Assert.ThrowsAsync<InsufficientDataException>(async () =>
            await repo.GetAverageWeeklyProgressAsync(user.Id, 4));
        
        Assert.Contains("Need at least 2 logs", exception.Message);
    }

    [Fact]
    public async Task GetAverageWeeklyProgressAsync_WithNoData_ShouldThrowInsufficientDataException()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();

        var exception = await Assert.ThrowsAsync<InsufficientDataException>(async () =>
            await repo.GetAverageWeeklyProgressAsync(user.Id, 4));
        
        Assert.Contains("No progress logs found", exception.Message);
    }
    
    [Fact]
    public async Task GetAverageWeeklyProgressAsync_WithOldDataOnly_ShouldThrowDataOutOfRangeException()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        // ایجاد لاگ‌های قدیمی (بیش از weeks هفته قبل)
        // این لاگ‌ها در startDate (today.AddDays(-28)) قرار نمی‌گیرند
        var veryOldDate1 = new DateOnly(2024, 1, 1);
        var veryOldDate2 = new DateOnly(2024, 1, 8);
        var veryOldDate3 = new DateOnly(2024, 1, 15);
        var veryOldDate4 = new DateOnly(2024, 1, 22);
        
        await CreateTestProgressLogWithWeightAsync(user.Id, veryOldDate1, null, 80f);
        await CreateTestProgressLogWithWeightAsync(user.Id, veryOldDate2, null, 79.5f);
        await CreateTestProgressLogWithWeightAsync(user.Id, veryOldDate3, null, 79f);
        await CreateTestProgressLogWithWeightAsync(user.Id, veryOldDate4, null, 78.5f);

        // این لاگ‌ها در startDate (4 هفته اخیر) نیستند
        // پس logs.Count = 0 خواهد بود و شرط logs.Count < 2 وارد می‌شود
        // سپس oldestLog وجود دارد و oldestLog.LogDate < startDate است
        // بنابراین باید DataOutOfRangeException پرتاب شود
        
        var exception = await Assert.ThrowsAsync<DataOutOfRangeException>(async () =>
            await repo.GetAverageWeeklyProgressAsync(user.Id, 4));
        
        Assert.Contains("Not enough recent data", exception.Message);
        Assert.Equal(4, exception.RequiredWeeks);
        Assert.True(exception.AvailableWeeks > 100, 
            $"Expected AvailableWeeks > 100, but got {exception.AvailableWeeks}");
    }

    [Fact]
    public async Task GetAverageWeeklyProgressAsync_WithTwoRecentLogs_ShouldReturnWeeklyChange()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var oneWeekAgo = today.AddDays(-7);
        
        await CreateTestProgressLogWithWeightAsync(user.Id, oneWeekAgo, null, 80f);
        await CreateTestProgressLogWithWeightAsync(user.Id, today, null, 78f);

        var weeklyChange = await repo.GetAverageWeeklyProgressAsync(user.Id, 1);

        Assert.NotNull(weeklyChange);
        Assert.Equal(-2f, weeklyChange.Value, 1);
    }

    [Fact]
    public async Task GetAverageWeeklyProgressAsync_WithEnoughData_ShouldReturnWeeklyChange()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        await CreateTestProgressLogWithWeightAsync(user.Id, today.AddDays(-28), null, 80f);
        await CreateTestProgressLogWithWeightAsync(user.Id, today.AddDays(-21), null, 79.5f);
        await CreateTestProgressLogWithWeightAsync(user.Id, today.AddDays(-14), null, 79f);
        await CreateTestProgressLogWithWeightAsync(user.Id, today.AddDays(-7), null, 78.5f);
        await CreateTestProgressLogWithWeightAsync(user.Id, today, null, 78f);

        var weeklyChange = await repo.GetAverageWeeklyProgressAsync(user.Id, 4);

        Assert.NotNull(weeklyChange);
        Assert.Equal(-0.5f, weeklyChange.Value, 1);
    }

    [Fact]
    public async Task GetWeightDifferenceAsync_ShouldReturnCorrectDifference()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestProgressLogWithWeightAsync(user.Id, new DateOnly(2024, 1, 1), null, 80f);
        await CreateTestProgressLogWithWeightAsync(user.Id, new DateOnly(2024, 1, 15), null, 78f);

        var difference = await repo.GetWeightDifferenceAsync(user.Id, new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 15));

        Assert.NotNull(difference);
        Assert.Equal(-2f, difference.Value);
    }

    [Fact]
    public async Task GetWeightDifferenceAsync_WhenLogsMissing_ShouldReturnNull()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        await CreateTestProgressLogWithWeightAsync(user.Id, new DateOnly(2024, 1, 1), null, 80f);

        var difference = await repo.GetWeightDifferenceAsync(user.Id, new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 15));

        Assert.Null(difference);
    }

    [Fact]
    public async Task GetAverageWeeklyProgressAsync_WithTwoRecentLogsButInsufficientWeeks_ShouldThrowInsufficientDataException()
    {
        var repo = new ProgressLogRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var twoDaysAgo = today.AddDays(-2);
        
        // دو لاگ جدید با فاصله 2 روز
        await CreateTestProgressLogWithWeightAsync(user.Id, twoDaysAgo, null, 80f);
        await CreateTestProgressLogWithWeightAsync(user.Id, today, null, 79.5f);

        // با دو لاگ و فاصله 2 روز، weeksSpan = 2/7 = 0.285 < 0.5
        // باید InsufficientDataException بگیریم
        var exception = await Assert.ThrowsAsync<InsufficientDataException>(async () =>
            await repo.GetAverageWeeklyProgressAsync(user.Id, 4));
        
        Assert.Contains("Insufficient time span", exception.Message);
    }
}