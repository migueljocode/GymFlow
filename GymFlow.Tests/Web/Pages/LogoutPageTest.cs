using GymFlow.Tests.Web.Pages.TestBase;
using GymFlow.Web.Pages;
using GymFlow.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymFlow.Tests.Web.Pages;

public class LogoutPageTest : PageModelTestFixture
{
    private readonly Mock<ApiClient> _mockApiClient;
    private readonly LogoutModel _pageModel;

    public LogoutPageTest()
    {
        _mockApiClient = new Mock<ApiClient>(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<ILogger<ApiClient>>(),
            Mock.Of<IConfiguration>(),
            Mock.Of<IHttpContextAccessor>())
        { CallBase = true };

        _pageModel = CreatePageModel<LogoutModel>(_mockApiClient.Object);
    }

    [Fact]
    public void OnGet_CallsApiClientLogout_ClearsSession_RedirectsToLogin()
    {
        // Arrange - تنظیم مقادیر در Session قبل از خروج
        SetSessionValue(_pageModel, "UserId", "123");
        SetSessionValue(_pageModel, "Username", "testuser");
        SetSessionValue(_pageModel, "UserRole", "Member");
        SetSessionValue(_pageModel, "AuthToken", "Basic token");

        // اطمینان از وجود مقادیر در Session
        Assert.Equal("123", GetSessionValue(_pageModel, "UserId"));
        Assert.Equal("testuser", GetSessionValue(_pageModel, "Username"));
        Assert.Equal("Member", GetSessionValue(_pageModel, "UserRole"));
        Assert.Equal("Basic token", GetSessionValue(_pageModel, "AuthToken"));

        // Act
        var result = _pageModel.OnGet();

        // Assert
        // 1. ApiClient.Logout باید فراخوانی شود
        _mockApiClient.Verify(c => c.Logout(), Times.Once);

        // 2. Session باید خالی باشد
        Assert.Null(GetSessionValue(_pageModel, "UserId"));
        Assert.Null(GetSessionValue(_pageModel, "Username"));
        Assert.Null(GetSessionValue(_pageModel, "UserRole"));
        Assert.Null(GetSessionValue(_pageModel, "AuthToken"));

        // 3. ریدایرکت به صفحه Login
        var redirectResult = AssertRedirectToPage(result, "/Login");
        Assert.NotNull(redirectResult);
    }

    [Fact]
    public void OnGet_WhenSessionAlreadyEmpty_StillWorks()
    {
        // Arrange - Session قبلاً خالی است (بدون تنظیم)

        // Act
        var result = _pageModel.OnGet();

        // Assert
        _mockApiClient.Verify(c => c.Logout(), Times.Once);
        Assert.Null(GetSessionValue(_pageModel, "UserId"));
        var redirectResult = AssertRedirectToPage(result, "/Login");
        Assert.NotNull(redirectResult);
    }
}