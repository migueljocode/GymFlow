namespace GymFlow.Tests.Services;

public class WorkoutAnalyticsServiceTest
{
    private readonly Mock<IWorkoutSessionRepository> _mockWorkoutSessionRepository;
    private readonly Mock<IWorkoutDayRepository> _mockWorkoutDayRepository;
    private readonly Mock<IWorkoutPlanRepository> _mockWorkoutPlanRepository;
    private readonly WorkoutAnalyticsService _service;

    public WorkoutAnalyticsServiceTest()
    {
        _mockWorkoutSessionRepository = new Mock<IWorkoutSessionRepository>();
        _mockWorkoutDayRepository = new Mock<IWorkoutDayRepository>();
        _mockWorkoutPlanRepository = new Mock<IWorkoutPlanRepository>();
        _service = new WorkoutAnalyticsService(
            _mockWorkoutSessionRepository.Object,
            _mockWorkoutDayRepository.Object,
            _mockWorkoutPlanRepository.Object);
    }

    // ========== Helper Methods ==========

    private List<WorkoutSession> CreateSessions(params (DateOnly date, int duration, string feeling)[] sessionData)
    {
        return sessionData.Select((s, idx) => new WorkoutSession
        {
            Id = idx + 1,
            WorkoutDayId = 1,
            ActualDate = s.date,
            ActualDurationMinutes = s.duration,
            Feeling = s.feeling,
            CreatedAt = DateTime.UtcNow
        }).ToList();
    }

    private WorkoutSession CreateSession(DateOnly date, int duration = 60, string feeling = "Good")
    {
        return new WorkoutSession
        {
            Id = 1,
            WorkoutDayId = 1,
            ActualDate = date,
            ActualDurationMinutes = duration,
            Feeling = feeling,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ========== GetConsistencyScoreAsync Tests ==========

    [Fact]
    public async Task GetConsistencyScoreAsync_WithNoSessions_ShouldReturnZero()
    {
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(new List<WorkoutSession>());

        var result = await _service.GetConsistencyScoreAsync(1);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetConsistencyScoreAsync_WithPerfectAttendance_ShouldReturnOneHundred()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>();
        for (int i = 0; i < 12; i++) // 4 weeks * 3 sessions = 12
        {
            sessions.Add(CreateSession(today.AddDays(-i * 2))); // roughly 3 per week
        }
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);

        var result = await _service.GetConsistencyScoreAsync(1, 4);

        Assert.Equal(100, result);
    }

    [Fact]
    public async Task GetConsistencyScoreAsync_WithPartialAttendance_ShouldReturnCorrectPercentage()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>
        {
            CreateSession(today),
            CreateSession(today.AddDays(-2)),
            CreateSession(today.AddDays(-5))
        };
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);

        // expected 3 sessions out of 12 (4 weeks * 3) = 25%
        var result = await _service.GetConsistencyScoreAsync(1, 4);

        Assert.Equal(25, result);
    }

    [Fact]
    public async Task GetConsistencyScoreAsync_ShouldCapAtOneHundred()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>();
        for (int i = 0; i < 20; i++) // more than expected
        {
            sessions.Add(CreateSession(today.AddDays(-i)));
        }
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);

        var result = await _service.GetConsistencyScoreAsync(1, 4);

        Assert.Equal(100, result);
    }

    // ========== GetCurrentStreakAsync Tests ==========

    [Fact]
    public async Task GetCurrentStreakAsync_WithNoSessions_ShouldReturnZero()
    {
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(new List<WorkoutSession>());

        var result = await _service.GetCurrentStreakAsync(1);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_WithConsecutiveDays_ShouldReturnStreak()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>
        {
            CreateSession(today),
            CreateSession(today.AddDays(-1)),
            CreateSession(today.AddDays(-2)),
            CreateSession(today.AddDays(-3))
        };
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);

        var result = await _service.GetCurrentStreakAsync(1);

        Assert.Equal(4, result);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_WithGap_ShouldReturnStreakOnlyFromToday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>
        {
            CreateSession(today),
            CreateSession(today.AddDays(-1)),
            CreateSession(today.AddDays(-3)), // gap on day -2
            CreateSession(today.AddDays(-4))
        };
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);

        var result = await _service.GetCurrentStreakAsync(1);

        Assert.Equal(2, result); // only today and yesterday
    }

    [Fact]
    public async Task GetCurrentStreakAsync_WithNoSessionToday_ShouldReturnZero()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>
        {
            CreateSession(today.AddDays(-1)),
            CreateSession(today.AddDays(-2))
        };
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);

        var result = await _service.GetCurrentStreakAsync(1);

        Assert.Equal(0, result);
    }

    // ========== GetLongestStreakAsync Tests ==========

    [Fact]
    public async Task GetLongestStreakAsync_WithNoSessions_ShouldReturnZero()
    {
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(new List<WorkoutSession>());

        var result = await _service.GetLongestStreakAsync(1);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetLongestStreakAsync_WithSingleSession_ShouldReturnOne()
    {
        var sessions = new List<WorkoutSession> { CreateSession(DateOnly.FromDateTime(DateTime.UtcNow)) };
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);

        var result = await _service.GetLongestStreakAsync(1);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetLongestStreakAsync_ShouldFindLongestConsecutiveDays()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>
        {
            CreateSession(today),
            CreateSession(today.AddDays(-1)),
            CreateSession(today.AddDays(-2)),
            CreateSession(today.AddDays(-4)), // gap
            CreateSession(today.AddDays(-5)),
            CreateSession(today.AddDays(-6)),
            CreateSession(today.AddDays(-7))
        };
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);

        var result = await _service.GetLongestStreakAsync(1);

        // longest streak is 4 days (today - 3 days back) or the 4-day streak at the end? Actually:
        // today, -1, -2 = 3 days
        // -4,-5,-6,-7 = 4 days
        Assert.Equal(4, result);
    }

    // ========== GetBestWorkoutDaysAsync Tests ==========

    [Fact]
    public async Task GetBestWorkoutDaysAsync_WithNoSessions_ShouldReturnEmptyDictionary()
    {
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(new List<WorkoutSession>());

        var result = await _service.GetBestWorkoutDaysAsync(1);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBestWorkoutDaysAsync_ShouldCountSessionsPerWeekday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>
        {
            CreateSession(today), // Monday? depends on today
            CreateSession(today.AddDays(1)),
            CreateSession(today.AddDays(1)),
            CreateSession(today.AddDays(2))
        };
        // Hard to predict weekday, so we'll use fixed dates for reliability
        var fixedDate = new DateOnly(2024, 1, 1); // Monday
        var sessionsFixed = new List<WorkoutSession>
        {
            CreateSession(fixedDate), // Monday
            CreateSession(fixedDate.AddDays(1)), // Tuesday
            CreateSession(fixedDate.AddDays(1)), // another Tuesday
            CreateSession(fixedDate.AddDays(2)) // Wednesday
        };
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessionsFixed);

        var result = await _service.GetBestWorkoutDaysAsync(1);

        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[DayOfWeek.Monday]);
        Assert.Equal(2, result[DayOfWeek.Tuesday]);
        Assert.Equal(1, result[DayOfWeek.Wednesday]);
    }

    // ========== GetUserAchievementsAsync Tests ==========

    [Fact]
    public async Task GetUserAchievementsAsync_WithNoWorkouts_ShouldReturnEmpty()
    {
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(new List<WorkoutSession>());
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(new List<WorkoutSession>()); // for totalWorkouts count

        var result = await _service.GetUserAchievementsAsync(1);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserAchievementsAsync_WithTenWorkouts_ShouldReturnGettingStarted()
    {
        var sessions = new List<WorkoutSession>();
        for (int i = 0; i < 10; i++)
        {
            sessions.Add(CreateSession(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i))));
        }
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions); // for total count

        var result = await _service.GetUserAchievementsAsync(1);

        Assert.Contains(result, a => a.Name == "Getting Started");
        Assert.DoesNotContain(result, a => a.Name == "Dedicated Athlete");
    }

    [Fact]
    public async Task GetUserAchievementsAsync_WithFiftyWorkouts_ShouldReturnDedicatedAthlete()
    {
        var sessions = new List<WorkoutSession>();
        for (int i = 0; i < 50; i++)
        {
            sessions.Add(CreateSession(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i))));
        }
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);

        var result = await _service.GetUserAchievementsAsync(1);

        Assert.Contains(result, a => a.Name == "Getting Started");
        Assert.Contains(result, a => a.Name == "Dedicated Athlete");
    }

    [Fact]
    public async Task GetUserAchievementsAsync_WithSevenDayStreak_ShouldReturnConsistencyKing()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>();
        for (int i = 0; i < 7; i++)
        {
            sessions.Add(CreateSession(today.AddDays(-i)));
        }
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);

        var result = await _service.GetUserAchievementsAsync(1);

        Assert.Contains(result, a => a.Name == "Consistency King");
    }

    [Fact]
    public async Task GetUserAchievementsAsync_WithThirtyDayStreak_ShouldReturnUnstoppable()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = new List<WorkoutSession>();
        for (int i = 0; i < 30; i++)
        {
            sessions.Add(CreateSession(today.AddDays(-i)));
        }
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);

        var result = await _service.GetUserAchievementsAsync(1);

        Assert.Contains(result, a => a.Name == "Unstoppable");
    }

    // ========== Placeholder Methods Tests ==========

    [Fact]
    public async Task GetCompletionRateByMuscleGroupAsync_ShouldReturnEmptyDictionary()
    {
        var result = await _service.GetCompletionRateByMuscleGroupAsync(1);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetVolumeTrendAsync_ShouldReturnEmptyList()
    {
        var result = await _service.GetVolumeTrendAsync(1);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}