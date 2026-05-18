namespace GymFlow.Tests.Web.Pages;

public class LoginPageTest : PageModelTestFixture
{
    private readonly Mock<ApiClient> _mockApiClient;
    private readonly LoginModel _pageModel;

    public LoginPageTest()
    {
        _mockApiClient = new Mock<ApiClient>(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<ILogger<ApiClient>>(),
            Mock.Of<IConfiguration>(),
            Mock.Of<IHttpContextAccessor>())
        { CallBase = true };

        _pageModel = CreatePageModel<LoginModel>(_mockApiClient.Object);
    }

    #region OnPostAsync

    [Fact]
    public async Task OnPostAsync_WhenUsernameAndPasswordEmpty_ReturnsPageWithError()
    {
        // Arrange
        _pageModel.Username = "";
        _pageModel.Password = "";

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("لطفاً نام کاربری و رمز عبور را وارد کنید", _pageModel.ErrorMessage);
        _mockApiClient.Verify(c => c.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenUsernameEmpty_ReturnsPageWithError()
    {
        // Arrange
        _pageModel.Username = "";
        _pageModel.Password = "pass123";

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("لطفاً نام کاربری و رمز عبور را وارد کنید", _pageModel.ErrorMessage);
        _mockApiClient.Verify(c => c.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenPasswordEmpty_ReturnsPageWithError()
    {
        // Arrange
        _pageModel.Username = "user";
        _pageModel.Password = "";

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("لطفاً نام کاربری و رمز عبور را وارد کنید", _pageModel.ErrorMessage);
        _mockApiClient.Verify(c => c.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenLoginFails_ReturnsPageWithError()
    {
        // Arrange
        _pageModel.Username = "wronguser";
        _pageModel.Password = "wrongpass";
        _mockApiClient.Setup(c => c.LoginAsync("wronguser", "wrongpass"))
            .ReturnsAsync((false, 0));

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("نام کاربری یا رمز عبور اشتباه است", _pageModel.ErrorMessage);
        Assert.Null(_pageModel.HttpContext.Session.GetString("Username"));
        Assert.Null(_pageModel.HttpContext.Session.GetString("UserRole"));
        Assert.Null(_pageModel.HttpContext.Session.GetString("UserId"));
    }

    [Fact]
    public async Task OnPostAsync_WhenMemberLoginSucceeds_SetsSessionAndRedirectsToIndex()
    {
        // Arrange
        _pageModel.Username = "member";
        _pageModel.Password = "member123";
        _mockApiClient.Setup(c => c.LoginAsync("member", "member123"))
            .ReturnsAsync((true, 42));

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        var redirectResult = AssertRedirectToPage(result, "/Index");
        Assert.NotNull(redirectResult);
        Assert.Equal("member", _pageModel.HttpContext.Session.GetString("Username"));
        Assert.Equal("Member", _pageModel.HttpContext.Session.GetString("UserRole"));
        Assert.Equal("42", _pageModel.HttpContext.Session.GetString("UserId"));
        Assert.Null(_pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenCoachLoginSucceeds_SetsRoleCoachAndRedirects()
    {
        // Arrange
        _pageModel.Username = "coach";
        _pageModel.Password = "coach123";
        _mockApiClient.Setup(c => c.LoginAsync("coach", "coach123"))
            .ReturnsAsync((true, 1));

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        var redirectResult = AssertRedirectToPage(result, "/Index");
        Assert.NotNull(redirectResult);
        Assert.Equal("coach", _pageModel.HttpContext.Session.GetString("Username"));
        Assert.Equal("Coach", _pageModel.HttpContext.Session.GetString("UserRole"));
        Assert.Equal("1", _pageModel.HttpContext.Session.GetString("UserId"));
    }

    #endregion
}