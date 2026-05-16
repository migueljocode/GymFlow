using Xunit;
using Moq;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Services.Implementations;
using GymFlow.Services.Interfaces;
using GymFlow.Services.Models;

namespace GymFlow.Tests.Services;

public class UserDashboardServiceTest
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IWorkoutPlanRepository> _mockWorkoutPlanRepository;
    private readonly Mock<IWorkoutSessionRepository> _mockWorkoutSessionRepository;
    private readonly Mock<IProgressLogRepository> _mockProgressLogRepository;
    private readonly Mock<IWorkoutAnalyticsService> _mockAnalyticsService;
    private readonly Mock<IWeightPredictionService> _mockPredictionService;
    private readonly UserDashboardService _userDashboardService;

    public UserDashboardServiceTest()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockWorkoutPlanRepository = new Mock<IWorkoutPlanRepository>();
        _mockWorkoutSessionRepository = new Mock<IWorkoutSessionRepository>();
        _mockProgressLogRepository = new Mock<IProgressLogRepository>();
        _mockAnalyticsService = new Mock<IWorkoutAnalyticsService>();
        _mockPredictionService = new Mock<IWeightPredictionService>();
        
        _userDashboardService = new UserDashboardService(
            _mockUserRepository.Object,
            _mockWorkoutPlanRepository.Object,
            _mockWorkoutSessionRepository.Object,
            _mockProgressLogRepository.Object,
            _mockAnalyticsService.Object,
            _mockPredictionService.Object);
    }

    // ========== Helper Methods ==========

    private User CreateTestUser(int id = 1)
    {
        var person = new Person
        {
            Id = id,
            FirstName = "Test",
            LastName = "User",
            Username = $"testuser{id}",
            Email = $"test{id}@test.com",
            Gender = Gender.Male,
            Age = 30,
            Weight = 80f,
            Height = 180f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow
        };

        return new User
        {
            Id = id,
            PersonId = person.Id,
            Person = person,
            Goal = Goal.Fitness,
            CreatedAt = DateTime.UtcNow
        };
    }

    private WorkoutPlan CreateTestWorkoutPlan(int id = 1, int userId = 1, bool isActive = true)
    {
        return new WorkoutPlan
        {
            Id = id,
            UserId = userId,
            Phase = 1,
            SessionsPerWeek = 3,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = null,
            IsActive = isActive,
            Notes = "Test plan",
            CreatedAt = DateTime.UtcNow
        };
    }

    private WorkoutSession CreateTestWorkoutSession(int id = 1, int workoutDayId = 1, DateOnly? actualDate = null)
    {
        var workoutDay = new WorkoutDay
        {
            Id = workoutDayId,
            DayOfWeek = DayOfWeek.Monday,
            TargetMuscles = MuscleGroup.Chest,
            DurationMinutes = 60,
            Intensity = Intensity.Medium
        };

        return new WorkoutSession
        {
            Id = id,
            WorkoutDayId = workoutDayId,
            WorkoutDay = workoutDay,
            ActualDate = actualDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            ActualDurationMinutes = 60,
            Feeling = "Great!",
            CreatedAt = DateTime.UtcNow
        };
    }

    private ProgressLog CreateTestProgressLog(int id = 1, int userId = 1, DateOnly? logDate = null, float weight = 75f)
    {
        return new ProgressLog
        {
            Id = id,
            UserId = userId,
            LogDate = logDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Weight = weight,
            BodyFatPercentage = 15.5f,
            Notes = "Test note",
            CreatedAt = DateTime.UtcNow
        };
    }

    private PredictionResponse CreateTestPredictionResponse()
    {
        return new PredictionResponse
        {
            CurrentWeight = 75f,
            PredictedWeight7Days = 74.5f,
            PredictedWeight30Days = 73f,
            Trend = "Losing",
            Confidence = "High",
            Message = "You're on track!"
        };
    }

    private List<Achievement> CreateTestAchievements()
    {
        return new List<Achievement>
        {
            new Achievement { Name = "First Workout", Description = "Completed first workout", Icon = "🎯" },
            new Achievement { Name = "5 Workouts", Description = "Completed 5 workouts", Icon = "🌟" }
        };
    }

    // ========== GetUserDashboardAsync Tests ==========

    [Fact]
    public async Task GetUserDashboardAsync_WithValidUserId_ShouldReturnDashboardData()
    {
        // Arrange
        var user = CreateTestUser(1);
        var activePlan = CreateTestWorkoutPlan(1, 1, true);
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, DateOnly.FromDateTime(DateTime.UtcNow)),
            CreateTestWorkoutSession(2, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)))
        };
        var logs = new List<ProgressLog>
        {
            CreateTestProgressLog(1, 1, DateOnly.FromDateTime(DateTime.UtcNow), 75f),
            CreateTestProgressLog(2, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)), 76f)
        };
        var prediction = CreateTestPredictionResponse();
        var achievements = CreateTestAchievements();

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);
        _mockWorkoutPlanRepository.Setup(r => r.GetActiveWorkoutPlanAsync(1))
            .ReturnsAsync(activePlan);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByDateRangeAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(sessions);
        _mockProgressLogRepository.Setup(r => r.GetUserProgressHistoryAsync(1))
            .ReturnsAsync(logs);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 10))
            .ReturnsAsync(logs);
        _mockPredictionService.Setup(r => r.GetPredictionAsync(1))
            .ReturnsAsync(prediction);
        _mockAnalyticsService.Setup(r => r.GetUserAchievementsAsync(1))
            .ReturnsAsync(achievements);
        _mockAnalyticsService.Setup(r => r.GetCurrentStreakAsync(1))
            .ReturnsAsync(3);
        _mockAnalyticsService.Setup(r => r.GetConsistencyScoreAsync(1))
            .ReturnsAsync(75);

        // Act
        var result = await _userDashboardService.GetUserDashboardAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Stats);
        Assert.NotNull(result.RecentActivities);
        Assert.NotNull(result.Achievements);
        Assert.NotNull(result.ActivePlan);
        Assert.NotNull(result.WeightPrediction);
        Assert.NotNull(result.WeeklySummary);
        Assert.NotNull(result.WeightHistory);
    }

    [Fact]
    public async Task GetUserDashboardAsync_WithInvalidUserId_ShouldThrowException()
    {
        // Arrange
        _mockUserRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _userDashboardService.GetUserDashboardAsync(999));
        Assert.Contains("User with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task GetUserDashboardAsync_WithoutActivePlan_ShouldReturnNullActivePlan()
    {
        // Arrange
        var user = CreateTestUser(1);
        var sessions = new List<WorkoutSession>();
        var logs = new List<ProgressLog>();
        var prediction = CreateTestPredictionResponse();
        var achievements = CreateTestAchievements();

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);
        _mockWorkoutPlanRepository.Setup(r => r.GetActiveWorkoutPlanAsync(1))
            .ReturnsAsync((WorkoutPlan?)null);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByDateRangeAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(sessions);
        _mockProgressLogRepository.Setup(r => r.GetUserProgressHistoryAsync(1))
            .ReturnsAsync(logs);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 10))
            .ReturnsAsync(logs);
        _mockPredictionService.Setup(r => r.GetPredictionAsync(1))
            .ReturnsAsync(prediction);
        _mockAnalyticsService.Setup(r => r.GetUserAchievementsAsync(1))
            .ReturnsAsync(achievements);
        _mockAnalyticsService.Setup(r => r.GetCurrentStreakAsync(1))
            .ReturnsAsync(0);
        _mockAnalyticsService.Setup(r => r.GetConsistencyScoreAsync(1))
            .ReturnsAsync(0);

        // Act
        var result = await _userDashboardService.GetUserDashboardAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ActivePlan);
    }

    // ========== GetQuickStatsAsync Tests ==========

    [Fact]
    public async Task GetQuickStatsAsync_WithValidUserId_ShouldReturnQuickStats()
    {
        // Arrange
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, DateOnly.FromDateTime(DateTime.UtcNow)),
            CreateTestWorkoutSession(2, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))),
            CreateTestWorkoutSession(3, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)))
        };
        var logs = new List<ProgressLog>
        {
            CreateTestProgressLog(1, 1, DateOnly.FromDateTime(DateTime.UtcNow), 75f),
            CreateTestProgressLog(2, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)), 76f),
            CreateTestProgressLog(3, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-14)), 77f)
        };

        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);
        _mockProgressLogRepository.Setup(r => r.GetUserProgressHistoryAsync(1))
            .ReturnsAsync(logs);
        _mockAnalyticsService.Setup(r => r.GetCurrentStreakAsync(1))
            .ReturnsAsync(5);
        _mockAnalyticsService.Setup(r => r.GetConsistencyScoreAsync(1))
            .ReturnsAsync(80);
        _mockAnalyticsService.Setup(r => r.GetUserAchievementsAsync(1))
            .ReturnsAsync(new List<Achievement>());

        // Act
        var result = await _userDashboardService.GetQuickStatsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalWorkouts);
        Assert.Equal(5, result.CurrentStreak);
        Assert.Equal(80, result.ConsistencyScore);
        Assert.Equal(75f, result.CurrentWeight);
        Assert.Equal(-2f, result.TotalWeightChange); // 75 - 77 = -2
    }

    [Fact]
    public async Task GetQuickStatsAsync_WithNoSessions_ShouldReturnZeroStats()
    {
        // Arrange
        var sessions = new List<WorkoutSession>();
        var logs = new List<ProgressLog>();

        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);
        _mockProgressLogRepository.Setup(r => r.GetUserProgressHistoryAsync(1))
            .ReturnsAsync(logs);
        _mockAnalyticsService.Setup(r => r.GetCurrentStreakAsync(1))
            .ReturnsAsync(0);
        _mockAnalyticsService.Setup(r => r.GetConsistencyScoreAsync(1))
            .ReturnsAsync(0);
        _mockAnalyticsService.Setup(r => r.GetUserAchievementsAsync(1))
            .ReturnsAsync(new List<Achievement>());

        // Act
        var result = await _userDashboardService.GetQuickStatsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalWorkouts);
        Assert.Equal(0, result.WorkoutsThisWeek);
        Assert.Equal(0, result.CurrentStreak);
        Assert.Equal(0, result.ConsistencyScore);
        Assert.Equal(0f, result.CurrentWeight);
        Assert.Equal(0f, result.TotalWeightChange);
    }

    // ========== GetRecentActivitiesAsync Tests ==========

    [Fact]
    public async Task GetRecentActivitiesAsync_ShouldReturnCombinedActivities()
    {
        // Arrange
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, DateOnly.FromDateTime(DateTime.UtcNow)),
            CreateTestWorkoutSession(2, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)))
        };
        var logs = new List<ProgressLog>
        {
            CreateTestProgressLog(1, 1, DateOnly.FromDateTime(DateTime.UtcNow), 75f),
            CreateTestProgressLog(2, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), 76f)
        };

        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);
        _mockProgressLogRepository.Setup(r => r.GetUserProgressHistoryAsync(1))
            .ReturnsAsync(logs);

        // Act
        var result = await _userDashboardService.GetRecentActivitiesAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        Assert.Contains(result, a => a.Type == "workout");
        Assert.Contains(result, a => a.Type == "weight");
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_WithLimitCount_ShouldReturnLimitedActivities()
    {
        // Arrange
        var sessions = new List<WorkoutSession>();
        for (int i = 1; i <= 20; i++)
        {
            sessions.Add(CreateTestWorkoutSession(i, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i))));
        }
        var logs = new List<ProgressLog>();

        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);
        _mockProgressLogRepository.Setup(r => r.GetUserProgressHistoryAsync(1))
            .ReturnsAsync(logs);

        // Act
        var result = await _userDashboardService.GetRecentActivitiesAsync(1, 5);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= 5);
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_WithNoData_ShouldReturnEmptyList()
    {
        // Arrange
        var sessions = new List<WorkoutSession>();
        var logs = new List<ProgressLog>();

        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);
        _mockProgressLogRepository.Setup(r => r.GetUserProgressHistoryAsync(1))
            .ReturnsAsync(logs);

        // Act
        var result = await _userDashboardService.GetRecentActivitiesAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_ShouldOrderByTimestampDescending()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, DateOnly.FromDateTime(now.AddDays(-5))),
            CreateTestWorkoutSession(2, 1, DateOnly.FromDateTime(now.AddDays(-1)))
        };
        var logs = new List<ProgressLog>
        {
            CreateTestProgressLog(1, 1, DateOnly.FromDateTime(now.AddDays(-3)), 75f)
        };

        sessions[0].CreatedAt = now.AddDays(-5);
        sessions[1].CreatedAt = now.AddDays(-1);
        logs[0].CreatedAt = now.AddDays(-3);

        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);
        _mockProgressLogRepository.Setup(r => r.GetUserProgressHistoryAsync(1))
            .ReturnsAsync(logs);

        // Act
        var result = await _userDashboardService.GetRecentActivitiesAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result[0].Timestamp >= result[1].Timestamp);
        Assert.True(result[1].Timestamp >= result[2].Timestamp);
    }

    // ========== WeeklySummary Tests ==========

    [Fact]
    public async Task GetUserDashboardAsync_WeeklySummary_ShouldCalculateCorrectPercentage()
    {
        // Arrange
        var user = CreateTestUser(1);
        var activePlan = CreateTestWorkoutPlan(1, 1, true);
        activePlan.SessionsPerWeek = 4;
        
        var sessions = new List<WorkoutSession>
        {
            CreateTestWorkoutSession(1, 1, DateOnly.FromDateTime(DateTime.UtcNow)),
            CreateTestWorkoutSession(2, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)))
        };
        var logs = new List<ProgressLog>();
        var prediction = CreateTestPredictionResponse();
        var achievements = CreateTestAchievements();

        _mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);
        _mockWorkoutPlanRepository.Setup(r => r.GetActiveWorkoutPlanAsync(1))
            .ReturnsAsync(activePlan);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByUserAsync(1))
            .ReturnsAsync(sessions);
        _mockWorkoutSessionRepository.Setup(r => r.GetSessionsByDateRangeAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(sessions);
        _mockProgressLogRepository.Setup(r => r.GetUserProgressHistoryAsync(1))
            .ReturnsAsync(logs);
        _mockProgressLogRepository.Setup(r => r.GetWeightTrendAsync(1, 10))
            .ReturnsAsync(logs);
        _mockPredictionService.Setup(r => r.GetPredictionAsync(1))
            .ReturnsAsync(prediction);
        _mockAnalyticsService.Setup(r => r.GetUserAchievementsAsync(1))
            .ReturnsAsync(achievements);
        _mockAnalyticsService.Setup(r => r.GetCurrentStreakAsync(1))
            .ReturnsAsync(2);
        _mockAnalyticsService.Setup(r => r.GetConsistencyScoreAsync(1))
            .ReturnsAsync(50);

        // Act
        var result = await _userDashboardService.GetUserDashboardAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.WeeklySummary);
        Assert.Equal(2, result.WeeklySummary.TotalSessions);
        // 2 out of 4 sessions = 50%
        Assert.Equal(50, result.WeeklySummary.CompletedPlanPercentage);
    }
}