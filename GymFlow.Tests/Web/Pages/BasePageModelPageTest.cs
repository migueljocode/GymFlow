using GymFlow.Web.Pages;
using GymFlow.Tests.Web.Pages.TestBase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Xunit;

namespace GymFlow.Tests.Web.Pages;

public class BasePageModelPageTest : PageModelTestFixture
{
    // یک کلاس مشتق شده برای تست متدهای protected
    private class TestPageModel : BasePageModel
    {
        // متد عمومی برای فراخوانی متد protected
        public IActionResult? TestRedirectIfNotLoggedIn() => RedirectIfNotLoggedIn();
    }

    [Fact]
    public void RedirectIfNotLoggedIn_WhenUserIsLoggedIn_ReturnsNull()
    {
        // Arrange
        var pageModel = CreatePageModel<TestPageModel>();
        SetAuthenticatedUser(pageModel, userId: 1, username: "testuser", role: "Member");

        // Act
        var result = pageModel.TestRedirectIfNotLoggedIn();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void RedirectIfNotLoggedIn_WhenUserIsNotLoggedIn_ReturnsRedirectToLogin()
    {
        // Arrange
        var pageModel = CreatePageModel<TestPageModel>();
        // Session را خالی بگذارید (بدون تنظیم UserId)

        // Act
        var result = pageModel.TestRedirectIfNotLoggedIn();

        // Assert
        var redirectResult = AssertRedirectToPage(result, "/Login");
        Assert.NotNull(redirectResult);
    }

    [Fact]
    public void RedirectIfNotLoggedIn_WhenUserIdIsEmptyString_ReturnsRedirectToLogin()
    {
        // Arrange
        var pageModel = CreatePageModel<TestPageModel>();
        // مقدار خالی برای UserId
        SetSessionValue(pageModel, "UserId", "");

        // Act
        var result = pageModel.TestRedirectIfNotLoggedIn();

        // Assert
        var redirectResult = AssertRedirectToPage(result, "/Login");
        Assert.NotNull(redirectResult);
    }

    [Fact]
    public void RedirectIfNotLoggedIn_WhenUserIdIsNull_ReturnsRedirectToLogin()
    {
        // Arrange
        var pageModel = CreatePageModel<TestPageModel>();
        // مطمئن شوید Session هیچ مقداری برای "UserId" ندارد (پیش‌فرض)

        // Act
        var result = pageModel.TestRedirectIfNotLoggedIn();

        // Assert
        var redirectResult = AssertRedirectToPage(result, "/Login");
        Assert.NotNull(redirectResult);
    }
}