namespace GymFlow.Tests.Dal.Seed.Generators;

public class ProgressGeneratorTest
{
    private readonly SeedOptions _options;

    public ProgressGeneratorTest()
    {
        _options = SeedProfiles.Lightweight;
        _options.RandomSeed = 42;
    }

    // ========== Helper Methods ==========

    private Person CreateTestPerson()
    {
        return new Person
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Username = "testuser",
            Password = "password123",
            Email = "test@test.com",
            Gender = Gender.Male,
            Age = 30,
            Weight = 80f,
            Height = 180f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow
        };
    }

    private User CreateTestUser(Person person, Goal goal = Goal.Fitness)
    {
        return new User
        {
            Id = 1,
            PersonId = person.Id,
            Person = person,
            Goal = goal,
            CreatedAt = DateTime.UtcNow
        };
    }

    private WorkoutPlan CreateTestWorkoutPlan(int userId, DateOnly startDate, DateOnly? endDate = null)
    {
        return new WorkoutPlan
        {
            Id = 1,
            UserId = userId,
            Phase = 1,
            SessionsPerWeek = 3,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private WorkoutDay CreateTestWorkoutDay(int planId, DayOfWeek dayOfWeek = DayOfWeek.Monday)
    {
        return new WorkoutDay
        {
            Id = 1,
            WorkoutPlanId = planId,
            DayOfWeek = dayOfWeek,
            TargetMuscles = MuscleGroup.Chest,
            DurationMinutes = 60,
            Intensity = Intensity.Medium,
            CreatedAt = DateTime.UtcNow
        };
    }

    private ProgressGenerator CreateGenerator(int startLogId = 1, int startSessionId = 1)
    {
        return new ProgressGenerator(_options, startLogId, startSessionId);
    }

    // ========== Tests for GenerateProgressLogs ==========

    [Fact]
    public void GenerateProgressLogs_ShouldReturnEmptyList_WhenDateRangeIsInvalid()
    {
        // Arrange
        var generator = CreateGenerator();
        var person = CreateTestPerson();
        var user = CreateTestUser(person);
        var plan = CreateTestWorkoutPlan(user.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), 
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))); // endDate < startDate

        // Act
        var logs = generator.GenerateProgressLogs(user, plan, person);

        // Assert
        Assert.Empty(logs);
    }

    [Fact]
    public void GenerateProgressLogs_ShouldGenerateCorrectNumberOfLogs()
    {
        // Arrange
        var generator = CreateGenerator();
        var person = CreateTestPerson();
        var user = CreateTestUser(person);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(user.Id, startDate, endDate);

        // Act
        var logs = generator.GenerateProgressLogs(user, plan, person);

        // Assert
        Assert.NotNull(logs);
        Assert.InRange(logs.Count, 1, 30);
    }

    [Fact]
    public void GenerateProgressLogs_ShouldHaveUniqueDates()
    {
        // Arrange
        var generator = CreateGenerator();
        var person = CreateTestPerson();
        var user = CreateTestUser(person);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(user.Id, startDate, endDate);

        // Act
        var logs = generator.GenerateProgressLogs(user, plan, person);
        var dates = logs.Select(l => l.LogDate).ToList();
        var uniqueDates = dates.Distinct().ToList();

        // Assert
        Assert.Equal(dates.Count, uniqueDates.Count);
    }

    [Fact]
    public void GenerateProgressLogs_ShouldHaveValidLogDates()
    {
        // Arrange
        var generator = CreateGenerator();
        var person = CreateTestPerson();
        var user = CreateTestUser(person);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(user.Id, startDate, endDate);

        // Act
        var logs = generator.GenerateProgressLogs(user, plan, person);

        // Assert
        foreach (var log in logs)
        {
            Assert.True(log.LogDate >= startDate);
            Assert.True(log.LogDate <= endDate);
        }
    }

    [Fact]
    public void GenerateProgressLogs_ShouldHaveValidWeightRange()
    {
        // Arrange
        var generator = CreateGenerator();
        var person = CreateTestPerson();
        var user = CreateTestUser(person);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(user.Id, startDate, endDate);

        // Act
        var logs = generator.GenerateProgressLogs(user, plan, person);

        // Assert
        foreach (var log in logs)
        {
            Assert.InRange(log.Weight, 45f, 130f);
        }
    }

    [Fact]
    public void GenerateProgressLogs_ForFatLossGoal_ShouldShowWeightDecrease()
    {
        // Arrange
        var generator = CreateGenerator();
        var person = CreateTestPerson();
        var user = CreateTestUser(person, Goal.FatLoss);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(user.Id, startDate, endDate);

        // Act
        var logs = generator.GenerateProgressLogs(user, plan, person);
        
        if (logs.Count >= 2)
        {
            var firstWeight = logs.First().Weight;
            var lastWeight = logs.Last().Weight;
            
            // Assert - وزن آخر باید کمتر از وزن اول باشد
            Assert.True(lastWeight <= firstWeight, 
                $"Expected weight loss: first={firstWeight}, last={lastWeight}");
        }
    }

    [Fact]
    public void GenerateProgressLogs_ForMuscleGainGoal_ShouldGenerateValidWeights()
    {
        // Arrange
        var generator = CreateGenerator();
        var person = CreateTestPerson();
        var user = CreateTestUser(person, Goal.MuscleGain);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(user.Id, startDate, endDate);

        // Act
        var logs = generator.GenerateProgressLogs(user, plan, person);

        // Assert
        foreach (var log in logs)
        {
            Assert.InRange(log.Weight, 45f, 130f);
        }
    }

    [Fact]
    public void GenerateProgressLogs_ShouldIncrementLogId()
    {
        // Arrange
        var generator = CreateGenerator(10, 1);
        var person = CreateTestPerson();
        var user = CreateTestUser(person);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(user.Id, startDate, endDate);

        // Act
        var logs = generator.GenerateProgressLogs(user, plan, person);
        var currentIdAfter = generator.CurrentLogId;

        // Assert
        if (logs.Any())
        {
            Assert.Equal(10 + logs.Count, currentIdAfter);
            Assert.Equal(10, logs.First().Id);
        }
    }

    [Fact]
    public void GenerateProgressLogs_MayHaveBodyFatPercentage()
    {
        // Arrange
        var generator = CreateGenerator();
        var person = CreateTestPerson();
        var user = CreateTestUser(person);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(user.Id, startDate, endDate);

        // Act
        var logs = generator.GenerateProgressLogs(user, plan, person);
        
        // Assert - بررسی می‌کند که اگر BodyFatPercentage دارد، در محدوده معتبر باشد
        foreach (var log in logs)
        {
            if (log.BodyFatPercentage.HasValue)
            {
                Assert.InRange(log.BodyFatPercentage.Value, 8f, 30f);
            }
        }
    }

    [Fact]
    public void GenerateProgressLogs_MayHaveNotes()
    {
        // Arrange
        var generator = CreateGenerator();
        var person = CreateTestPerson();
        var user = CreateTestUser(person);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(user.Id, startDate, endDate);

        // Act
        var logs = generator.GenerateProgressLogs(user, plan, person);
        Assert.NotNull(logs);

        // Assert - بررسی می‌کند که اگر Notes دارد، خالی نباشد
        foreach (var log in logs)
        {
            if (!string.IsNullOrEmpty(log.Notes))
            {
                Assert.NotEmpty(log.Notes);
            }
        }
    }

    [Fact]
    public void GenerateProgressLogs_ShouldHaveCreatedAtEqualToLogDate()
    {
        // Arrange
        var generator = CreateGenerator();
        var person = CreateTestPerson();
        var user = CreateTestUser(person);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(user.Id, startDate, endDate);

        // Act
        var logs = generator.GenerateProgressLogs(user, plan, person);

        // Assert
        foreach (var log in logs)
        {
            Assert.Equal(log.LogDate.ToDateTime(TimeOnly.MinValue), log.CreatedAt);
        }
    }

    // ========== Tests for GenerateWorkoutSessions ==========

    [Fact]
    public void GenerateWorkoutSessions_ShouldGenerateSessionsForValidDateRange()
    {
        // Arrange
        var generator = CreateGenerator(1, 10);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(1, startDate, endDate);
        var workoutDay = CreateTestWorkoutDay(plan.Id, DayOfWeek.Monday);

        // Act
        var sessions = generator.GenerateWorkoutSessions(workoutDay, plan);

        // Assert
        Assert.NotNull(sessions);
        // ممکن است همه هفته‌ها session نداشته باشد
        foreach (var session in sessions)
        {
            Assert.Equal(DayOfWeek.Monday, session.ActualDate.DayOfWeek);
        }
    }

    [Fact]
    public void GenerateWorkoutSessions_ShouldHaveValidActualDates()
    {
        // Arrange
        var generator = CreateGenerator();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(1, startDate, endDate);
        var workoutDay = CreateTestWorkoutDay(plan.Id, DayOfWeek.Wednesday);

        // Act
        var sessions = generator.GenerateWorkoutSessions(workoutDay, plan);

        // Assert
        foreach (var session in sessions)
        {
            Assert.True(session.ActualDate >= startDate);
            Assert.True(session.ActualDate <= endDate);
            Assert.Equal(DayOfWeek.Wednesday, session.ActualDate.DayOfWeek);
        }
    }

    [Fact]
    public void GenerateWorkoutSessions_ShouldHaveValidDurationMinutes()
    {
        // Arrange
        var generator = CreateGenerator();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(1, startDate, endDate);
        var workoutDay = CreateTestWorkoutDay(plan.Id, DayOfWeek.Friday);

        // Act
        var sessions = generator.GenerateWorkoutSessions(workoutDay, plan);

        // Assert
        foreach (var session in sessions)
        {
            Assert.InRange(session.ActualDurationMinutes, workoutDay.DurationMinutes - 10, 
                workoutDay.DurationMinutes + 15);
        }
    }

    [Fact]
    public void GenerateWorkoutSessions_ShouldIncrementSessionId()
    {
        // Arrange
        var generator = CreateGenerator(1, 50);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(1, startDate, endDate);
        var workoutDay = CreateTestWorkoutDay(plan.Id, DayOfWeek.Monday);

        // Act
        var sessions = generator.GenerateWorkoutSessions(workoutDay, plan);
        var currentIdAfter = generator.CurrentSessionId;

        // Assert
        if (sessions.Any())
        {
            Assert.Equal(50 + sessions.Count, currentIdAfter);
            Assert.Equal(50, sessions.First().Id);
        }
    }

    [Fact]
    public void GenerateWorkoutSessions_MayHaveFeeling()
    {
        // Arrange
        var generator = CreateGenerator();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(1, startDate, endDate);
        var workoutDay = CreateTestWorkoutDay(plan.Id, DayOfWeek.Monday);

        // Act
        var sessions = generator.GenerateWorkoutSessions(workoutDay, plan);
        var validFeelings = new[] { "Energetic", "Good session", "Tired", "Great pump", 
                                    "Knee slightly sore", "Amazing energy", "Decent workout", 
                                    "Personal best", "Felt weak", "Crushed it" };

        // Assert - بررسی می‌کند که اگر Feeling دارد، معتبر باشد
        foreach (var session in sessions.Where(s => !string.IsNullOrEmpty(s.Feeling)))
        {
            var feelingContainsValid = validFeelings.Any(vf => session.Feeling!.Contains(vf));
            Assert.True(feelingContainsValid, $"Feeling '{session.Feeling}' not in valid list");
        }
    }

    [Fact]
    public void GenerateWorkoutSessions_ShouldHaveCreatedAtAfterOrEqualToActualDate()
    {
        // Arrange
        var generator = CreateGenerator();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(1, startDate, endDate);
        var workoutDay = CreateTestWorkoutDay(plan.Id, DayOfWeek.Monday);

        // Act
        var sessions = generator.GenerateWorkoutSessions(workoutDay, plan);

        // Assert
        foreach (var session in sessions)
        {
            var minCreatedAt = session.ActualDate.ToDateTime(TimeOnly.MinValue).AddHours(6);
            var maxCreatedAt = session.ActualDate.ToDateTime(TimeOnly.MinValue).AddHours(20);
            Assert.True(session.CreatedAt >= minCreatedAt);
            Assert.True(session.CreatedAt <= maxCreatedAt);
        }
    }

    [Fact]
    public void GenerateWorkoutSessions_WithNoEndDate_ShouldUseCurrentDate()
    {
        // Arrange
        var generator = CreateGenerator();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var plan = CreateTestWorkoutPlan(1, startDate, null); // No end date
        var workoutDay = CreateTestWorkoutDay(plan.Id, DayOfWeek.Monday);

        // Act
        var sessions = generator.GenerateWorkoutSessions(workoutDay, plan);

        // Assert
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var session in sessions)
        {
            Assert.True(session.ActualDate <= today);
        }
    }

    // ========== Tests for CurrentId Properties ==========

    [Fact]
    public void CurrentLogId_ShouldStartWithCorrectValue()
    {
        // Arrange & Act
        var generator1 = CreateGenerator(1, 1);
        var generator2 = CreateGenerator(50, 100);
        var generator3 = CreateGenerator(200, 300);

        // Assert
        Assert.Equal(1, generator1.CurrentLogId);
        Assert.Equal(50, generator2.CurrentLogId);
        Assert.Equal(200, generator3.CurrentLogId);
    }

    [Fact]
    public void CurrentSessionId_ShouldStartWithCorrectValue()
    {
        // Arrange & Act
        var generator1 = CreateGenerator(1, 1);
        var generator2 = CreateGenerator(50, 100);
        var generator3 = CreateGenerator(200, 300);

        // Assert
        Assert.Equal(1, generator1.CurrentSessionId);
        Assert.Equal(100, generator2.CurrentSessionId);
        Assert.Equal(300, generator3.CurrentSessionId);
    }

    // ========== Integration Tests ==========

    [Fact]
    public void GenerateMultipleProgressLogs_ShouldGenerateUniqueIds()
    {
        // Arrange
        var generator = CreateGenerator(1, 1);
        var person = CreateTestPerson();
        var user = CreateTestUser(person);
        
        var plan1 = CreateTestWorkoutPlan(user.Id, 
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2)), 
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)));
        var plan2 = CreateTestWorkoutPlan(user.Id, 
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)), 
            DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var logs1 = generator.GenerateProgressLogs(user, plan1, person);
        var logs2 = generator.GenerateProgressLogs(user, plan2, person);
        
        var allIds = logs1.Select(l => l.Id).Concat(logs2.Select(l => l.Id)).ToList();
        var distinctIds = allIds.Distinct().ToList();

        // Assert
        Assert.Equal(allIds.Count, distinctIds.Count);
    }

    [Fact]
    public void GenerateMultipleWorkoutSessions_ShouldGenerateUniqueIds()
    {
        // Arrange
        var generator = CreateGenerator(1, 1);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = CreateTestWorkoutPlan(1, startDate, endDate);
        var workoutDay1 = CreateTestWorkoutDay(plan.Id, DayOfWeek.Monday);
        var workoutDay2 = CreateTestWorkoutDay(plan.Id, DayOfWeek.Wednesday);

        // Act
        var sessions1 = generator.GenerateWorkoutSessions(workoutDay1, plan);
        var sessions2 = generator.GenerateWorkoutSessions(workoutDay2, plan);
        
        var allIds = sessions1.Select(s => s.Id).Concat(sessions2.Select(s => s.Id)).ToList();
        var distinctIds = allIds.Distinct().ToList();

        // Assert
        Assert.Equal(allIds.Count, distinctIds.Count);
    }
}