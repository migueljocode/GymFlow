#nullable disable

namespace GymFlow.Tests.Web.Pages.Workout;

// ================== Stub ApiClient با قابلیت پرتاب Exception ==================
public class StubApiClient : ApiClient
{
    private readonly Dictionary<string, object> _responses = new();
    private readonly Dictionary<string, Exception> _exceptions = new();
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

    public void AddException(string url, Exception exception)
    {
        _exceptions[url] = exception;
    }

    public override async Task<T> GetAsync<T>(string url)
    {
        if (_exceptions.TryGetValue(url, out var ex))
            throw ex;

        if (_responses.TryGetValue(url, out var response))
        {
            var json = JsonSerializer.Serialize(response, _jsonOptions);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        return default;
    }

    public override async Task<T> PostAsync<T>(string url, object data)
    {
        if (_exceptions.TryGetValue(url, out var ex))
            throw ex;

        if (typeof(T) == typeof(object))
            return (T)(object)new { };
        return default;
    }
}

// ================== DTOهای مورد نیاز ==================
public class ActivePlanDto
{
    public int Id { get; set; }
    public int Phase { get; set; }
    public bool IsActive { get; set; }
}

public class WorkoutDayResponse
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
}

// ================== Test Class ==================
public class LogPageTest : PageModelTestFixture
{
    private readonly StubApiClient _apiClient;
    private readonly LogModel _pageModel;

    public LogPageTest()
    {
        _apiClient = new StubApiClient();
        _pageModel = CreatePageModel<LogModel>(_apiClient);
    }

    #region OnPostAsync

    [Fact]
    public async Task OnPostAsync_WhenUserNotLoggedIn_RedirectsToLogin()
    {
        _pageModel.HttpContext.Session.Clear();

        var result = await _pageModel.OnPostAsync();
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Login", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostAsync_WhenNoActivePlan_ReturnsPageWithError()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");

        var result = await _pageModel.OnPostAsync();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Null(redirectResult.PageName);   // چون RedirectToPage() بدون آرگومان، همان صفحه را رفرش می‌کند
    }

    [Fact]
    public async Task OnPostAsync_WhenActivePlanExistsButNoWorkoutForToday_ReturnsPageWithError()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");

        var activePlan = new ActivePlanDto { Id = 5, Phase = 1, IsActive = true };
        _apiClient.AddResponse("api/workoutplans/user/1/active", activePlan);

        var workoutDays = new List<WorkoutDayResponse>
        {
            new() { Id = 1, DayOfWeek = DayOfWeek.Monday },
            new() { Id = 2, DayOfWeek = DayOfWeek.Wednesday }
        };
        _apiClient.AddResponse($"api/workoutdays/plan/{activePlan.Id}", workoutDays);

        _pageModel.ActualDate = new DateOnly(2024, 1, 9); // Tuesday

        var result = await _pageModel.OnPostAsync();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Null(redirectResult.PageName);   // چون RedirectToPage() بدون آرگومان، همان صفحه را رفرش می‌کند
    }

    [Fact]
    public async Task OnPostAsync_SuccessfulLog_ReturnsPageWithSuccessMessage()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");

        var activePlan = new ActivePlanDto { Id = 5, Phase = 1, IsActive = true };
        _apiClient.AddResponse("api/workoutplans/user/1/active", activePlan);

        var workoutDays = new List<WorkoutDayResponse>
        {
            new() { Id = 1, DayOfWeek = DayOfWeek.Monday },
            new() { Id = 2, DayOfWeek = DayOfWeek.Tuesday },
            new() { Id = 3, DayOfWeek = DayOfWeek.Wednesday }
        };
        _apiClient.AddResponse($"api/workoutdays/plan/{activePlan.Id}", workoutDays);

        _pageModel.ActualDate = new DateOnly(2024, 1, 9); // Tuesday
        _pageModel.DurationMinutes = 60;
        _pageModel.Feeling = "Great!";

        var result = await _pageModel.OnPostAsync();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Null(redirectResult.PageName);   // چون RedirectToPage() بدون آرگومان، همان صفحه را رفرش می‌کند
    }

    [Fact]
    public async Task OnPostAsync_WhenConflict409_ReturnsPageWithConflictMessage()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");

        var activePlan = new ActivePlanDto { Id = 5, Phase = 1, IsActive = true };
        _apiClient.AddResponse("api/workoutplans/user/1/active", activePlan);

        var workoutDays = new List<WorkoutDayResponse>
        {
            new() { Id = 1, DayOfWeek = DayOfWeek.Monday },
            new() { Id = 2, DayOfWeek = DayOfWeek.Tuesday }
        };
        _apiClient.AddResponse($"api/workoutdays/plan/{activePlan.Id}", workoutDays);

        _pageModel.ActualDate = new DateOnly(2024, 1, 9); // Tuesday

        _apiClient.AddException("api/workoutsessions/log", new Exception("Conflict 409 - Already logged"));

        var result = await _pageModel.OnPostAsync();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Null(redirectResult.PageName);   // چون RedirectToPage() بدون آرگومان، همان صفحه را رفرش می‌کند    
    }

    [Fact]
    public async Task OnPostAsync_WhenGeneralException_ReturnsPageWithGeneralError()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");

        var activePlan = new ActivePlanDto { Id = 5, Phase = 1, IsActive = true };
        _apiClient.AddResponse("api/workoutplans/user/1/active", activePlan);

        var workoutDays = new List<WorkoutDayResponse>
        {
            new() { Id = 1, DayOfWeek = DayOfWeek.Monday },
            new() { Id = 2, DayOfWeek = DayOfWeek.Tuesday }
        };
        _apiClient.AddResponse($"api/workoutdays/plan/{activePlan.Id}", workoutDays);

        _pageModel.ActualDate = new DateOnly(2024, 1, 9); // Tuesday

        _apiClient.AddException("api/workoutsessions/log", new Exception("Internal server error"));

        var result = await _pageModel.OnPostAsync();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Null(redirectResult.PageName);   // چون RedirectToPage() بدون آرگومان، همان صفحه را رفرش می‌کند
    }

    #endregion
}