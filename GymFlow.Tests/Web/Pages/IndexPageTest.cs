using GymFlow.Tests.Web.Pages.TestBase;
using GymFlow.Web.Pages;
using GymFlow.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymFlow.Tests.Web.Pages;

public class IndexPageTest : PageModelTestFixture
{
    private readonly Mock<ApiClient> _mockApiClient;
    private readonly IndexModel _pageModel;

    public IndexPageTest()
    {
        _mockApiClient = new Mock<ApiClient>(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<ILogger<ApiClient>>(),
            Mock.Of<IConfiguration>(),
            Mock.Of<IHttpContextAccessor>())
        { CallBase = true };

        _pageModel = CreatePageModel<IndexModel>(_mockApiClient.Object);
    }

    #region OnGetAsync

    [Fact]
    public async Task OnGetAsync_WhenUserNotLoggedIn_RedirectsToLogin()
    {
        // Arrange - بدون تنظیم Session

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        Assert.Equal(302, _pageModel.Response.StatusCode);
        Assert.Equal("/Login", _pageModel.Response.Headers["Location"].ToString());
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_LoadsUserData()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser", role: "Member");

        var quickStats = new QuickStatsDto { TotalWorkouts = 10, CurrentStreak = 3, ConsistencyScore = 75, CurrentWeight = 78.5f };
        _mockApiClient.Setup(c => c.GetAsync<QuickStatsDto>("api/statistics/user/1/quick-stats"))
            .ReturnsAsync(quickStats);

        var activePlan = new ActivePlanDto { Id = 5, Phase = 2, IsActive = true };
        _mockApiClient.Setup(c => c.GetAsync<ActivePlanDto>("api/workoutplans/user/1/active"))
            .ReturnsAsync(activePlan);

        var workoutDays = new List<WorkoutDayApiDto>
        {
            new() { Id = 1, DayOfWeek = DayOfWeek.Saturday, TargetMuscles = 1, DurationMinutes = 60, Intensity = 1 }
        };
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutDayApiDto>>("api/workoutdays/plan/5"))
            .ReturnsAsync(workoutDays);

        var progressLogs = new List<ProgressLogDto>
        {
            new() { LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)), Weight = 80f, CreatedAt = DateTime.UtcNow.AddDays(-7) },
            new() { LogDate = DateOnly.FromDateTime(DateTime.UtcNow), Weight = 78f, CreatedAt = DateTime.UtcNow }
        };
        _mockApiClient.Setup(c => c.GetAsync<List<ProgressLogDto>>("api/progress/user/1"))
            .ReturnsAsync(progressLogs);

        var sessions = new List<WorkoutSessionDto>
        {
            new() { ActualDate = DateOnly.FromDateTime(DateTime.UtcNow), ActualDurationMinutes = 60, CreatedAt = DateTime.UtcNow, Feeling = "Great" }
        };
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutSessionDto>>("api/workoutsessions/user/1"))
            .ReturnsAsync(sessions);

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        Assert.Equal("Member", _pageModel.UserRole);
        Assert.Equal("testuser", _pageModel.Username);
        Assert.False(_pageModel.IsCoach);
        Assert.True(_pageModel.IsMember);

        Assert.NotNull(_pageModel.Stats);
        Assert.Equal(10, _pageModel.Stats.TotalWorkouts);

        Assert.NotNull(_pageModel.TodayWorkout);
        Assert.Equal(60, _pageModel.TodayWorkout.DurationMinutes);

        Assert.NotNull(_pageModel.WeightHistory);
        Assert.Equal(2, _pageModel.WeightHistory.Count);

        Assert.NotNull(_pageModel.Achievements);
        Assert.Contains(_pageModel.Achievements, a => a.Name == "اولین تمرین");

        Assert.NotNull(_pageModel.RecentActivities);
        Assert.Equal(3, _pageModel.RecentActivities.Count);

        Assert.Equal("فاز 2", _pageModel.ActivePlanName);
    }

    [Fact]
    public async Task OnGetAsync_WhenNoActivePlan_ActivePlanNameShowsNoPlan()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, 1, "testuser");
        
        // Setup برای همه درخواست‌های GetAsync که در این تست استفاده می‌شوند
        _mockApiClient.Setup(c => c.GetAsync<QuickStatsDto>(It.IsAny<string>()))
            .ReturnsAsync(new QuickStatsDto());
        _mockApiClient.Setup(c => c.GetAsync<ActivePlanDto>("api/workoutplans/user/1/active"))
            .ReturnsAsync((ActivePlanDto)null!);
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutDayApiDto>>(It.IsAny<string>()))
            .ReturnsAsync(new List<WorkoutDayApiDto>());
        _mockApiClient.Setup(c => c.GetAsync<List<ProgressLogDto>>(It.IsAny<string>()))
            .ReturnsAsync(new List<ProgressLogDto>());
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutSessionDto>>(It.IsAny<string>()))
            .ReturnsAsync(new List<WorkoutSessionDto>());

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        Assert.Equal("برنامه فعالی ندارید", _pageModel.ActivePlanName);
        Assert.Null(_pageModel.TodayWorkout);
    }

    #endregion

    #region LoadWeightHistoryAsync

    [Fact]
    public async Task LoadWeightHistoryAsync_WhenLogsExist_OrdersByDate()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, 1, "testuser");
        var logs = new List<ProgressLogDto>
        {
            new() { LogDate = new DateOnly(2024, 1, 10), Weight = 80f },
            new() { LogDate = new DateOnly(2024, 1, 1), Weight = 82f },
            new() { LogDate = new DateOnly(2024, 1, 5), Weight = 81f }
        };
        _mockApiClient.Setup(c => c.GetAsync<List<ProgressLogDto>>("api/progress/user/1"))
            .ReturnsAsync(logs);
        _mockApiClient.Setup(c => c.GetAsync<QuickStatsDto>(It.IsAny<string>()))
            .ReturnsAsync(new QuickStatsDto());
        _mockApiClient.Setup(c => c.GetAsync<ActivePlanDto>(It.IsAny<string>()))
            .ReturnsAsync((ActivePlanDto)null!);
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutDayApiDto>>(It.IsAny<string>()))
            .ReturnsAsync(new List<WorkoutDayApiDto>());
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutSessionDto>>(It.IsAny<string>()))
            .ReturnsAsync(new List<WorkoutSessionDto>());

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        Assert.NotNull(_pageModel.WeightHistory);
        Assert.Equal(3, _pageModel.WeightHistory.Count);
        Assert.Equal(new DateOnly(2024, 1, 1), _pageModel.WeightHistory[0].Date);
        Assert.Equal(new DateOnly(2024, 1, 5), _pageModel.WeightHistory[1].Date);
        Assert.Equal(new DateOnly(2024, 1, 10), _pageModel.WeightHistory[2].Date);
    }

    [Fact]
    public async Task LoadWeightHistoryAsync_WhenNoLogs_ReturnsEmptyList()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, 1, "testuser");
        _mockApiClient.Setup(c => c.GetAsync<List<ProgressLogDto>>("api/progress/user/1"))
            .ReturnsAsync((List<ProgressLogDto>)null!);
        _mockApiClient.Setup(c => c.GetAsync<QuickStatsDto>(It.IsAny<string>()))
            .ReturnsAsync(new QuickStatsDto());
        _mockApiClient.Setup(c => c.GetAsync<ActivePlanDto>(It.IsAny<string>()))
            .ReturnsAsync((ActivePlanDto)null!);
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutDayApiDto>>(It.IsAny<string>()))
            .ReturnsAsync(new List<WorkoutDayApiDto>());
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutSessionDto>>(It.IsAny<string>()))
            .ReturnsAsync(new List<WorkoutSessionDto>());

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        Assert.NotNull(_pageModel.WeightHistory);
        Assert.Empty(_pageModel.WeightHistory);
    }

    #endregion

    #region LoadAchievementsAsync

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(5, 2)]
    [InlineData(10, 3)]
    [InlineData(25, 4)]
    [InlineData(50, 5)]
    [InlineData(100, 6)]
    public async Task LoadAchievementsAsync_ReturnsCorrectAchievements(int totalWorkouts, int expectedCount)
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, 1, "testuser");
        var sessions = Enumerable.Range(0, totalWorkouts)
            .Select(i => new WorkoutSessionDto { CreatedAt = DateTime.UtcNow.AddDays(-i) })
            .ToList();
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutSessionDto>>("api/workoutsessions/user/1"))
            .ReturnsAsync(sessions);
        _mockApiClient.Setup(c => c.GetAsync<QuickStatsDto>(It.IsAny<string>()))
            .ReturnsAsync(new QuickStatsDto());
        _mockApiClient.Setup(c => c.GetAsync<ActivePlanDto>(It.IsAny<string>()))
            .ReturnsAsync((ActivePlanDto)null!);
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutDayApiDto>>(It.IsAny<string>()))
            .ReturnsAsync(new List<WorkoutDayApiDto>());
        _mockApiClient.Setup(c => c.GetAsync<List<ProgressLogDto>>(It.IsAny<string>()))
            .ReturnsAsync(new List<ProgressLogDto>());

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        Assert.NotNull(_pageModel.Achievements);
        Assert.Equal(expectedCount, _pageModel.Achievements.Count);
    }

    #endregion

    #region LoadRecentActivitiesAsync

    [Fact]
    public async Task LoadRecentActivitiesAsync_CombinesSessionsAndLogs_OrdersDescending()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, 1, "testuser");
        var now = DateTime.UtcNow;
        var sessions = new List<WorkoutSessionDto>
        {
            new() { CreatedAt = now.AddHours(-1), ActualDurationMinutes = 60, Feeling = "Great" }
        };
        var logs = new List<ProgressLogDto>
        {
            new() { CreatedAt = now.AddHours(-2), Weight = 78f }
        };
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutSessionDto>>("api/workoutsessions/user/1"))
            .ReturnsAsync(sessions);
        _mockApiClient.Setup(c => c.GetAsync<List<ProgressLogDto>>("api/progress/user/1"))
            .ReturnsAsync(logs);
        _mockApiClient.Setup(c => c.GetAsync<QuickStatsDto>(It.IsAny<string>()))
            .ReturnsAsync(new QuickStatsDto());
        _mockApiClient.Setup(c => c.GetAsync<ActivePlanDto>(It.IsAny<string>()))
            .ReturnsAsync((ActivePlanDto)null!);
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutDayApiDto>>(It.IsAny<string>()))
            .ReturnsAsync(new List<WorkoutDayApiDto>());

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        Assert.NotNull(_pageModel.RecentActivities);
        Assert.Equal(2, _pageModel.RecentActivities.Count);
        Assert.Equal("تمرین ثبت شد", _pageModel.RecentActivities[0].Title);
        Assert.Equal("وزن ثبت شد", _pageModel.RecentActivities[1].Title);
    }

    #endregion
}