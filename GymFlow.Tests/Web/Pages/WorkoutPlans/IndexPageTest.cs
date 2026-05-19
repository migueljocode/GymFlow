#nullable disable
using IndexModel = GymFlow.Web.Pages.WorkoutPlans.IndexModel;

namespace GymFlow.Tests.Web.Pages.WorkoutPlans;

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
        { CallBase = false };

        _pageModel = CreatePageModel<IndexModel>(_mockApiClient.Object);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserNotLoggedIn_RedirectsToLogin()
    {
        // بدون تنظیم Session
        var result = await _pageModel.OnGetAsync();
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Login", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_NoPlans_ReturnsEmptyList()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutPlanListResponse>>(It.IsAny<string>()))
            .ReturnsAsync((List<WorkoutPlanListResponse>)null);

        var result = await _pageModel.OnGetAsync();
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.WorkoutPlans);
        Assert.Empty(_pageModel.WorkoutPlans);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_HasPlans_ReturnsList()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");
        var plans = new List<WorkoutPlanListResponse>
        {
            new() { Id = 1, Phase = 1, SessionsPerWeek = 3, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), IsActive = true },
            new() { Id = 2, Phase = 2, SessionsPerWeek = 4, StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)), IsActive = false }
        };
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutPlanListResponse>>(It.IsAny<string>()))
            .ReturnsAsync(plans);

        var result = await _pageModel.OnGetAsync();
        Assert.IsType<PageResult>(result);
        Assert.Equal(2, _pageModel.WorkoutPlans.Count);
        Assert.Equal(1, _pageModel.WorkoutPlans[0].Id);
        Assert.Equal(2, _pageModel.WorkoutPlans[1].Phase);
        Assert.True(_pageModel.WorkoutPlans[0].IsActive);
        Assert.False(_pageModel.WorkoutPlans[1].IsActive);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserIdInvalid_RedirectsToLogin()
    {
        SetSessionValue(_pageModel, "UserId", "invalid");
        var result = await _pageModel.OnGetAsync();
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Login", redirectResult.PageName);
    }
}