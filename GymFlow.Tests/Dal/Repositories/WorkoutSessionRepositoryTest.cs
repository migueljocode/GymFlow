namespace GymFlow.Tests.Dal.Repositories;

public class WorkoutSessionRepositoryTest : IClassFixture<DbContextFixture>
{
    private readonly DbContextFixture _fixture;
    private static int _uniqueCounter = 0;

    public WorkoutSessionRepositoryTest(DbContextFixture fixture)
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

    private async Task<WorkoutDay> CreateTestWorkoutDayAsync(int workoutPlanId, DayOfWeek dayOfWeek = DayOfWeek.Monday)
    {
        var repo = new WorkoutDayRepository(_fixture.DbContextFactory);
        var workoutDay = new WorkoutDay
        {
            WorkoutPlanId = workoutPlanId,
            DayOfWeek = dayOfWeek,
            TargetMuscles = MuscleGroup.Chest,
            DurationMinutes = 60,
            Intensity = Intensity.Medium,
            CreatedAt = DateTime.UtcNow
        };
        return await repo.AddAsync(workoutDay);
    }

    private async Task<WorkoutSession> CreateTestWorkoutSessionAsync(int workoutDayId, DateOnly? actualDate = null, int durationMinutes = 60)
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        var date = actualDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        
        // بررسی اینکه آیا برای این workoutDay و این تاریخ قبلاً سشن وجود دارد
        var existing = await repo.HasUserCompletedWorkoutDayAsync(workoutDayId, date);
        if (existing)
        {
            // اگر وجود داشت، یک روز بعد را امتحان کن
            date = date.AddDays(1);
        }
        
        var session = new WorkoutSession
        {
            WorkoutDayId = workoutDayId,
            ActualDate = date,
            ActualDurationMinutes = durationMinutes,
            Feeling = "Good workout!",
            CreatedAt = DateTime.UtcNow
        };
        return await repo.AddAsync(session);
    }

    private async Task<List<WorkoutSession>> CreateMultipleWorkoutSessionsAsync(int workoutDayId, int count, int startDayOffset = 0)
    {
        var sessions = new List<WorkoutSession>();
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        var baseDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(startDayOffset);
        
        for (int i = 0; i < count; i++)
        {
            var date = baseDate.AddDays(i);
            var session = new WorkoutSession
            {
                WorkoutDayId = workoutDayId,
                ActualDate = date,
                ActualDurationMinutes = 60 + (i * 5),
                Feeling = $"Workout {i + 1}",
                CreatedAt = DateTime.UtcNow
            };
            sessions.Add(await repo.AddAsync(session));
        }
        
        return sessions;
    }

    // ========== Generic Repository Query Tests ==========

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectWorkoutSession()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        var session = await CreateTestWorkoutSessionAsync(workoutDay.Id);

        var fetched = await repo.GetByIdAsync(session.Id);

        Assert.NotNull(fetched);
        Assert.Equal(session.Id, fetched.Id);
        Assert.Equal(session.ActualDurationMinutes, fetched.ActualDurationMinutes);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var fetched = await repo.GetByIdAsync(99999);

        Assert.Null(fetched);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatchingSession()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await CreateTestWorkoutSessionAsync(workoutDay.Id, today);
        await CreateTestWorkoutSessionAsync(workoutDay.Id, today.AddDays(1));

        var fetched = await repo.FirstOrDefaultAsync(ws => ws.ActualDate == today);

        Assert.NotNull(fetched);
        Assert.Equal(today, fetched.ActualDate);
    }

    [Fact]
    public async Task SingleOrDefaultAsync_WithSingleMatch_ShouldReturnSession()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        var session = await CreateTestWorkoutSessionAsync(workoutDay.Id);

        var fetched = await repo.SingleOrDefaultAsync(ws => ws.Id == session.Id);

        Assert.NotNull(fetched);
        Assert.Equal(session.Id, fetched.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSessions()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        await CreateMultipleWorkoutSessionsAsync(workoutDay.Id, 2);

        var all = await repo.GetAllAsync();

        Assert.NotNull(all);
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task FindAsync_ShouldReturnFilteredSessions()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        await CreateTestWorkoutSessionAsync(workoutDay.Id, null, 60);
        await CreateTestWorkoutSessionAsync(workoutDay.Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), 75);

        var found = await repo.FindAsync(ws => ws.ActualDurationMinutes == 60);

        Assert.Single(found);
        Assert.Equal(60, found.First().ActualDurationMinutes);
    }

    [Fact]
    public async Task FindAsync_ById_ShouldReturnSession()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        var session = await CreateTestWorkoutSessionAsync(workoutDay.Id);

        var fetched = await repo.FindAsync(session.Id);

        Assert.NotNull(fetched);
        Assert.Equal(session.Id, fetched.Id);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrueWhenExists()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        await CreateTestWorkoutSessionAsync(workoutDay.Id);

        var exists = await repo.AnyAsync(ws => ws.WorkoutDayId == workoutDay.Id);

        Assert.True(exists);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnFalseWhenNotExists()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var exists = await repo.AnyAsync(ws => ws.Id == 99999);

        Assert.False(exists);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnTrueWhenAllMatch()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        await CreateMultipleWorkoutSessionsAsync(workoutDay.Id, 2);

        var allMatch = await repo.AllAsync(ws => ws.WorkoutDayId == workoutDay.Id);

        Assert.True(allMatch);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnFalseWhenNotAllMatch()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan1 = await CreateTestWorkoutPlanAsync(user.Id);
        var plan2 = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay1 = await CreateTestWorkoutDayAsync(plan1.Id);
        var workoutDay2 = await CreateTestWorkoutDayAsync(plan2.Id);
        
        await CreateTestWorkoutSessionAsync(workoutDay1.Id);
        await CreateTestWorkoutSessionAsync(workoutDay2.Id);

        var allMatch = await repo.AllAsync(ws => ws.WorkoutDayId == workoutDay1.Id);

        Assert.False(allMatch);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnTotalCount()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        await CreateMultipleWorkoutSessionsAsync(workoutDay.Id, 2);

        var count = await repo.CountAsync();

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        await CreateTestWorkoutSessionAsync(workoutDay.Id, null, 60);
        await CreateTestWorkoutSessionAsync(workoutDay.Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), 60);
        await CreateTestWorkoutSessionAsync(workoutDay.Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2), 75);

        var count = await repo.CountAsync(ws => ws.ActualDurationMinutes == 60);

        Assert.Equal(2, count);
    }

    // ========== Generic Repository Command Tests ==========

    [Fact]
    public async Task AddAsync_ShouldSaveWorkoutSessionToDatabase()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        var session = new WorkoutSession
        {
            WorkoutDayId = workoutDay.Id,
            ActualDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ActualDurationMinutes = 90,
            Feeling = "Amazing!",
            CreatedAt = DateTime.UtcNow
        };

        var added = await repo.AddAsync(session);

        var fetched = await repo.GetByIdAsync(added.Id);
        Assert.NotNull(fetched);
        Assert.Equal(90, fetched.ActualDurationMinutes);
        Assert.Equal("Amazing!", fetched.Feeling);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingSession()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        var session = await CreateTestWorkoutSessionAsync(workoutDay.Id);
        
        session.ActualDurationMinutes = 120;
        session.Feeling = "Tired but great!";

        var updated = await repo.UpdateAsync(session);

        var fetched = await repo.GetByIdAsync(updated.Id);
        Assert.Equal(120, fetched?.ActualDurationMinutes);
        Assert.Equal("Tired but great!", fetched?.Feeling);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteSession()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        var session = await CreateTestWorkoutSessionAsync(workoutDay.Id);

        var deleted = await repo.DeleteAsync(session);

        Assert.True(deleted);
        
        var fetched = await repo.GetByIdAsync(session.Id);
        Assert.Null(fetched);
        
        await using var context = _fixture.CreateContext();
        var allSessions = await context.WorkoutSessions.IgnoreQueryFilters().ToListAsync();
        var deletedSession = allSessions.FirstOrDefault(ws => ws.Id == session.Id);
        Assert.NotNull(deletedSession);
        Assert.True(deletedSession.IsDeleted);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldSoftDeleteSessionById()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        var session = await CreateTestWorkoutSessionAsync(workoutDay.Id);

        var deleted = await repo.DeleteByIdAsync(session.Id);

        Assert.True(deleted);
        var fetched = await repo.GetByIdAsync(session.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithInvalidId_ShouldReturnFalse()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();

        var deleted = await repo.DeleteByIdAsync(99999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleSessions()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>
        {
            new() { WorkoutDayId = workoutDay.Id, ActualDate = today, ActualDurationMinutes = 60, CreatedAt = DateTime.UtcNow },
            new() { WorkoutDayId = workoutDay.Id, ActualDate = today.AddDays(1), ActualDurationMinutes = 75, CreatedAt = DateTime.UtcNow }
        };

        var added = await repo.AddRangeAsync(sessions);

        Assert.Equal(2, added.Count());
        var all = await repo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldSoftDeleteMultipleSessions()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        var sessions = await CreateMultipleWorkoutSessionsAsync(workoutDay.Id, 2);

        var deleted = await repo.DeleteRangeAsync(sessions);

        Assert.True(deleted);
        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task DeleteAllAsync_ShouldSoftDeleteAllSessions()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        await CreateMultipleWorkoutSessionsAsync(workoutDay.Id, 2);

        var deleted = await repo.DeleteAllAsync();

        Assert.True(deleted);
        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    // ========== Specific Interface Tests ==========

    [Fact]
    public async Task GetSessionsByUserAsync_ShouldReturnAllSessionsForUser()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user1 = await CreateTestUserAsync();
        var user2 = await CreateTestUserAsync();
        
        var plan1 = await CreateTestWorkoutPlanAsync(user1.Id);
        var plan2 = await CreateTestWorkoutPlanAsync(user2.Id);
        
        var workoutDay1 = await CreateTestWorkoutDayAsync(plan1.Id);
        var workoutDay2 = await CreateTestWorkoutDayAsync(plan2.Id);
        
        await CreateMultipleWorkoutSessionsAsync(workoutDay1.Id, 2);
        await CreateTestWorkoutSessionAsync(workoutDay2.Id);

        var sessionsForUser1 = await repo.GetSessionsByUserAsync(user1.Id);
        var sessionsForUser2 = await repo.GetSessionsByUserAsync(user2.Id);

        Assert.Equal(2, sessionsForUser1.Count());
        Assert.Single(sessionsForUser2);
    }

    [Fact]
    public async Task GetSessionsByWorkoutDayAsync_ShouldReturnAllSessionsForWorkoutDay()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay1 = await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Monday);
        var workoutDay2 = await CreateTestWorkoutDayAsync(plan.Id, DayOfWeek.Tuesday);
        
        await CreateMultipleWorkoutSessionsAsync(workoutDay1.Id, 2);
        await CreateTestWorkoutSessionAsync(workoutDay2.Id);

        var sessionsForDay1 = await repo.GetSessionsByWorkoutDayAsync(workoutDay1.Id);
        var sessionsForDay2 = await repo.GetSessionsByWorkoutDayAsync(workoutDay2.Id);

        Assert.Equal(2, sessionsForDay1.Count());
        Assert.Single(sessionsForDay2);
    }

    [Fact]
    public async Task GetSessionsByDateRangeAsync_ShouldReturnSessionsWithinDateRange()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        await CreateTestWorkoutSessionAsync(workoutDay.Id, today.AddDays(-5));
        await CreateTestWorkoutSessionAsync(workoutDay.Id, today.AddDays(-3));
        await CreateTestWorkoutSessionAsync(workoutDay.Id, today.AddDays(2));
        await CreateTestWorkoutSessionAsync(workoutDay.Id, today.AddDays(5));

        var startDate = today.AddDays(-4);
        var endDate = today.AddDays(3);
        
        var sessionsInRange = await repo.GetSessionsByDateRangeAsync(user.Id, startDate, endDate);

        Assert.Equal(2, sessionsInRange.Count());
        Assert.All(sessionsInRange, s => 
            Assert.True(s.ActualDate >= startDate && s.ActualDate <= endDate));
    }

    [Fact]
    public async Task GetLatestSessionAsync_ShouldReturnMostRecentSession()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        await CreateTestWorkoutSessionAsync(workoutDay.Id, today.AddDays(-10));
        await CreateTestWorkoutSessionAsync(workoutDay.Id, today.AddDays(-5));
        await CreateTestWorkoutSessionAsync(workoutDay.Id, today.AddDays(-1));

        var latest = await repo.GetLatestSessionAsync(user.Id);

        Assert.NotNull(latest);
        Assert.Equal(today.AddDays(-1), latest.ActualDate);
    }

    [Fact]
    public async Task GetLatestSessionAsync_WithNoSessions_ShouldReturnNull()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();

        var latest = await repo.GetLatestSessionAsync(user.Id);

        Assert.Null(latest);
    }

    [Fact]
    public async Task GetSessionCountByUserAsync_ShouldReturnTotalSessionCount()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        await CreateMultipleWorkoutSessionsAsync(workoutDay.Id, 3);

        var count = await repo.GetSessionCountByUserAsync(user.Id);

        Assert.Equal(3, count);
    }

    [Fact]
    public async Task GetSessionCountByUserAsync_WithFromDate_ShouldReturnCountSinceDate()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        await CreateTestWorkoutSessionAsync(workoutDay.Id, today.AddDays(-10));
        await CreateTestWorkoutSessionAsync(workoutDay.Id, today.AddDays(-3));
        await CreateTestWorkoutSessionAsync(workoutDay.Id, today.AddDays(-1));

        var count = await repo.GetSessionCountByUserAsync(user.Id, today.AddDays(-5));

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetAverageSessionDurationAsync_ShouldReturnAverageDuration()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        await CreateTestWorkoutSessionAsync(workoutDay.Id, null, 60);
        await CreateTestWorkoutSessionAsync(workoutDay.Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), 75);
        await CreateTestWorkoutSessionAsync(workoutDay.Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2), 90);

        var average = await repo.GetAverageSessionDurationAsync(user.Id);

        Assert.Equal(75, average, 1); // (60 + 75 + 90) / 3 = 75
    }

    [Fact]
    public async Task GetAverageSessionDurationAsync_WithNoSessions_ShouldThrowNoDataFoundException()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();

        var exception = await Assert.ThrowsAsync<NoDataFoundException>(async () =>
            await repo.GetAverageSessionDurationAsync(user.Id));
        
        Assert.Contains("No workout sessions found", exception.Message);
        Assert.Equal(nameof(WorkoutSession), exception.EntityName);
        Assert.Contains($"UserId = {user.Id}", exception.FilterInfo);
    }

    [Fact]
    public async Task HasUserCompletedWorkoutDayAsync_WithCompletedWorkout_ShouldReturnTrue()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await CreateTestWorkoutSessionAsync(workoutDay.Id, today);

        var hasCompleted = await repo.HasUserCompletedWorkoutDayAsync(workoutDay.Id, today);

        Assert.True(hasCompleted);
    }

    [Fact]
    public async Task HasUserCompletedWorkoutDayAsync_WithNoCompletion_ShouldReturnFalse()
    {
        var repo = new WorkoutSessionRepository(_fixture.DbContextFactory);
        await repo.DeleteAllAsync();
        
        var user = await CreateTestUserAsync();
        var plan = await CreateTestWorkoutPlanAsync(user.Id);
        var workoutDay = await CreateTestWorkoutDayAsync(plan.Id);
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);
        await CreateTestWorkoutSessionAsync(workoutDay.Id, yesterday);

        var hasCompleted = await repo.HasUserCompletedWorkoutDayAsync(workoutDay.Id, today);

        Assert.False(hasCompleted);
    }
}