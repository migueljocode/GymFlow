using GymFlow.Tests.Web.Pages.TestBase;
using GymFlow.Web.Pages;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GymFlow.Tests.Web.Pages;

public class BasePageModelPageTest : PageModelTestFixture
{
    private class TestPageModel : BasePageModel
    {
        public IActionResult? TestRedirectIfNotMember() => RedirectIfNotMember();
        public IActionResult? TestRedirectIfNotCoach() => RedirectIfNotCoach();
    }

    [Fact]
    public void RedirectIfNotMember_WhenUserIsMember_ReturnsNull()
    {
        var pageModel = CreatePageModel<TestPageModel>();
        SetAuthenticatedUser(pageModel, userId: 1, username: "member", role: "Member");

        var result = pageModel.TestRedirectIfNotMember();

        Assert.Null(result);
    }

    [Fact]
    public void RedirectIfNotMember_WhenUserIsCoach_ReturnsRedirectToLogin()
    {
        var pageModel = CreatePageModel<TestPageModel>();
        SetAuthenticatedUser(pageModel, userId: 2, username: "coach", role: "Coach");

        var result = pageModel.TestRedirectIfNotMember();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Login", redirectResult.PageName);
    }

    [Fact]
    public void RedirectIfNotCoach_WhenUserIsCoach_ReturnsNull()
    {
        var pageModel = CreatePageModel<TestPageModel>();
        SetAuthenticatedUser(pageModel, userId: 2, username: "coach", role: "Coach");

        var result = pageModel.TestRedirectIfNotCoach();

        Assert.Null(result);
    }

    [Fact]
    public void RedirectIfNotCoach_WhenUserIsMember_ReturnsRedirectToLogin()
    {
        var pageModel = CreatePageModel<TestPageModel>();
        SetAuthenticatedUser(pageModel, userId: 1, username: "member", role: "Member");

        var result = pageModel.TestRedirectIfNotCoach();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Login", redirectResult.PageName);
    }
}