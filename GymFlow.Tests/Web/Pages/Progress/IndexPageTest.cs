#nullable disable

using GymFlow.Tests.Web.Pages.TestBase;
using GymFlow.Web.Pages.Progress;
using GymFlow.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymFlow.Tests.Web.Pages.Progress;

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
        // بدون تنظیم Session (UserId وجود ندارد)

        await _pageModel.OnGetAsync();

        // بررسی ریدایرکت
        Assert.Equal(302, _pageModel.Response.StatusCode);
        Assert.Equal("/Login", _pageModel.Response.Headers["Location"].ToString());
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_NoData_EmptyHistoryAndZeroStats()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");
        _mockApiClient.Setup(c => c.GetAsync<List<WeightLogDto>>("api/progress/user/1"))
            .ReturnsAsync((List<WeightLogDto>)null);

        await _pageModel.OnGetAsync();

        Assert.NotNull(_pageModel.WeightHistory);
        Assert.Empty(_pageModel.WeightHistory);
        Assert.Equal(0, _pageModel.CurrentWeight);
        Assert.Equal(0, _pageModel.FirstWeight);
        Assert.Equal(0, _pageModel.TotalChange);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_HasData_SetsHistoryAndCalculatesWeights()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");

        var logs = new List<WeightLogDto>
        {
            new() { LogDate = new DateOnly(2024, 1, 10), Weight = 78f },
            new() { LogDate = new DateOnly(2024, 1, 5), Weight = 80f },
            new() { LogDate = new DateOnly(2024, 1, 1), Weight = 82f }
        };
        _mockApiClient.Setup(c => c.GetAsync<List<WeightLogDto>>("api/progress/user/1"))
            .ReturnsAsync(logs);

        await _pageModel.OnGetAsync();

        Assert.Equal(3, _pageModel.WeightHistory.Count);
        // ترتیب اصلی بر اساس تاریخ نزولی نیست – مدل خودش OrderByDescending می‌کند
        // بنابراین بررسی می‌کنیم که CurrentWeight جدیدترین وزن باشد (78)
        Assert.Equal(78f, _pageModel.CurrentWeight);
        Assert.Equal(82f, _pageModel.FirstWeight);
        Assert.Equal(-4f, _pageModel.TotalChange);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_SingleEntry_CalculatesCorrectly()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");

        var logs = new List<WeightLogDto>
        {
            new() { LogDate = new DateOnly(2024, 1, 10), Weight = 75f }
        };
        _mockApiClient.Setup(c => c.GetAsync<List<WeightLogDto>>("api/progress/user/1"))
            .ReturnsAsync(logs);

        await _pageModel.OnGetAsync();

        Assert.Single(_pageModel.WeightHistory);
        Assert.Equal(75f, _pageModel.CurrentWeight);
        Assert.Equal(75f, _pageModel.FirstWeight);
        Assert.Equal(0, _pageModel.TotalChange);
    }

    #endregion
}