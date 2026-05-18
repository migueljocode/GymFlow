#nullable disable

namespace GymFlow.Tests.Web.Pages;

// ================== Stub ApiClient ==================
public class StubApiClient : ApiClient
{
    private readonly Dictionary<string, object> _responses = new();
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public StubApiClient() : base(
        Mock.Of<IHttpClientFactory>(),
        Mock.Of<ILogger<ApiClient>>(),
        Mock.Of<IConfiguration>(),
        Mock.Of<IHttpContextAccessor>())
    { }

    public void AddResponse<T>(string url, T data)
    {
        _responses[url] = data;
    }

    public override async Task<T> GetAsync<T>(string url)
    {
        if (_responses.TryGetValue(url, out var response))
        {
            var json = JsonSerializer.Serialize(response, _jsonOptions);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        return default;
    }

    public override async Task<T> PostAsync<T>(string url, object data) => default;
    public override async Task<bool> PutAsync(string url, object data) => true;
    public override async Task<bool> DeleteAsync(string url) => true;
    public override async Task<byte[]> DownloadPdfAsync(string url) => null;
}

// ================== DTOهای مورد نیاز ==================
public class QuickStatsDto
{
    public int TotalWorkouts { get; set; }
    public int CurrentStreak { get; set; }
    public int ConsistencyScore { get; set; }
    public float CurrentWeight { get; set; }
}

public class ActivePlanDto
{
    public int Id { get; set; }
    public int Phase { get; set; }
    public bool IsActive { get; set; }
}

public class WorkoutDayApiDto
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public int TargetMuscles { get; set; }
    public int DurationMinutes { get; set; }
    public int Intensity { get; set; }
}

public class ProgressLogDto
{
    public DateOnly LogDate { get; set; }
    public float Weight { get; set; }
    public float? BodyFatPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WorkoutSessionDto
{
    public DateOnly ActualDate { get; set; }
    public int ActualDurationMinutes { get; set; }
    public string Feeling { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ================== Test Class ==================
public class IndexPageTest : PageModelTestFixture
{
    private readonly StubApiClient _apiClient;
    private readonly IndexModel _pageModel;

    public IndexPageTest()
    {
        _apiClient = new StubApiClient();
        _pageModel = CreatePageModel<IndexModel>(_apiClient);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserNotLoggedIn_RedirectsToLogin()
    {
        await _pageModel.OnGetAsync();
        Assert.Equal(302, _pageModel.Response.StatusCode);
        Assert.Equal("/Login", _pageModel.Response.Headers["Location"].ToString());
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_LoadsUserData()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser", role: "Member");

        var quickStats = new QuickStatsDto { TotalWorkouts = 10, CurrentStreak = 3, ConsistencyScore = 75, CurrentWeight = 78.5f };
        _apiClient.AddResponse("api/statistics/user/1/quick-stats", quickStats);

        var activePlan = new ActivePlanDto { Id = 5, Phase = 2, IsActive = true };
        _apiClient.AddResponse("api/workoutplans/user/1/active", activePlan);

        // تمام روزهای هفته
        var workoutDays = new List<WorkoutDayApiDto>
        {
            new() { Id = 1, DayOfWeek = DayOfWeek.Saturday, TargetMuscles = 1, DurationMinutes = 60, Intensity = 1 },
            new() { Id = 2, DayOfWeek = DayOfWeek.Sunday, TargetMuscles = 1, DurationMinutes = 60, Intensity = 1 },
            new() { Id = 3, DayOfWeek = DayOfWeek.Monday, TargetMuscles = 1, DurationMinutes = 60, Intensity = 1 },
            new() { Id = 4, DayOfWeek = DayOfWeek.Tuesday, TargetMuscles = 1, DurationMinutes = 60, Intensity = 1 },
            new() { Id = 5, DayOfWeek = DayOfWeek.Wednesday, TargetMuscles = 1, DurationMinutes = 60, Intensity = 1 },
            new() { Id = 6, DayOfWeek = DayOfWeek.Thursday, TargetMuscles = 1, DurationMinutes = 60, Intensity = 1 },
            new() { Id = 7, DayOfWeek = DayOfWeek.Friday, TargetMuscles = 1, DurationMinutes = 60, Intensity = 1 }
        };
        _apiClient.AddResponse("api/workoutdays/plan/5", workoutDays);

        var logs = new List<ProgressLogDto>
        {
            new() { LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)), Weight = 80f, CreatedAt = DateTime.UtcNow.AddDays(-7) },
            new() { LogDate = DateOnly.FromDateTime(DateTime.UtcNow), Weight = 78f, CreatedAt = DateTime.UtcNow }
        };
        _apiClient.AddResponse("api/progress/user/1", logs);

        var sessions = new List<WorkoutSessionDto>
        {
            new() { ActualDate = DateOnly.FromDateTime(DateTime.UtcNow), ActualDurationMinutes = 60, CreatedAt = DateTime.UtcNow, Feeling = "Great" }
        };
        _apiClient.AddResponse("api/workoutsessions/user/1", sessions);

        await _pageModel.OnGetAsync();

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
        Assert.Equal(3, _pageModel.RecentActivities.Count); // 1 session + 2 logs

        Assert.Equal("فاز 2", _pageModel.ActivePlanName);
    }

    [Fact]
    public async Task OnGetAsync_WhenNoActivePlan_ActivePlanNameShowsNoPlan()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");

        _apiClient.AddResponse("api/statistics/user/1/quick-stats", new QuickStatsDto());
        _apiClient.AddResponse("api/workoutplans/user/1/active", (ActivePlanDto)null);
        _apiClient.AddResponse("api/workoutdays/plan/0", new List<WorkoutDayApiDto>());
        _apiClient.AddResponse("api/progress/user/1", new List<ProgressLogDto>());
        _apiClient.AddResponse("api/workoutsessions/user/1", new List<WorkoutSessionDto>());

        await _pageModel.OnGetAsync();

        Assert.Equal("برنامه فعالی ندارید", _pageModel.ActivePlanName);
        Assert.Null(_pageModel.TodayWorkout);
    }
}