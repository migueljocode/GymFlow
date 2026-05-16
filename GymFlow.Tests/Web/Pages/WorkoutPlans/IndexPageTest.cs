#nullable disable

using GymFlow.Tests.Web.Pages.TestBase;
using GymFlow.Web.Pages.WorkoutPlans;
using GymFlow.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

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
        { CallBase = true };

        _pageModel = CreatePageModel<IndexModel>(_mockApiClient.Object);
    }

    #region OnGetAsync

    [Fact]
    public async Task OnGetAsync_WhenUserNotLoggedIn_RedirectsToLogin()
    {
        // Arrange - بدون Session (UserId وجود ندارد)

        // Act
        await _pageModel.OnGetAsync();

        // Assert - بررسی ریدایرکت (متد Response.Redirect باعث تنظیم Location header و StatusCode می‌شود)
        Assert.Equal(302, _pageModel.Response.StatusCode);
        Assert.Equal("/Login", _pageModel.Response.Headers["Location"].ToString());
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_NoPlans_ReturnsEmptyList()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutPlanListDto>>("api/workoutplans/user/1"))
            .ReturnsAsync((List<WorkoutPlanListDto>)null);

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        Assert.NotNull(_pageModel.WorkoutPlans);
        Assert.Empty(_pageModel.WorkoutPlans);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserLoggedIn_HasPlans_ReturnsList()
    {
        // Arrange
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");
        var plans = new List<WorkoutPlanListDto>
        {
            new() { Id = 1, Phase = 1, SessionsPerWeek = 3, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), IsActive = true },
            new() { Id = 2, Phase = 2, SessionsPerWeek = 4, StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)), IsActive = false }
        };
        _mockApiClient.Setup(c => c.GetAsync<List<WorkoutPlanListDto>>("api/workoutplans/user/1"))
            .ReturnsAsync(plans);

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        Assert.NotNull(_pageModel.WorkoutPlans);
        Assert.Equal(2, _pageModel.WorkoutPlans.Count);
        Assert.Equal(1, _pageModel.WorkoutPlans[0].Id);
        Assert.Equal(2, _pageModel.WorkoutPlans[1].Phase);
        Assert.True(_pageModel.WorkoutPlans[0].IsActive);
        Assert.False(_pageModel.WorkoutPlans[1].IsActive);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserIdInvalid_RedirectsToLogin()
    {
        // Arrange - تنظیم Session با مقدار غیر عددی برای UserId
        SetSessionValue(_pageModel, "UserId", "invalid");

        // Act
        await _pageModel.OnGetAsync();

        // Assert
        Assert.Equal(302, _pageModel.Response.StatusCode);
        Assert.Equal("/Login", _pageModel.Response.Headers["Location"].ToString());
        _mockApiClient.Verify(c => c.GetAsync<List<WorkoutPlanListDto>>(It.IsAny<string>()), Times.Never);
    }

    #endregion
}