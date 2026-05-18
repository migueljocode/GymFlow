namespace GymFlow.Tests.Api.Controllers;

public class TestAuthControllerTest : ControllerTestFixture
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly TestAuthController _controller;

    public TestAuthControllerTest()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = CreateController<TestAuthController>(_mockAuthService.Object);
    }

    #region CheckUserAsync

    [Fact]
    public async Task CheckUserAsync_ValidCredentials_ReturnsUserInfo()
    {
        // Arrange
        var user = new User
        {
            Id = 5,
            Person = new Person
            {
                Username = "testuser",
                Password = "secret123",
                FirstName = "Test",
                LastName = "User"
            }
        };
        _mockAuthService.Setup(s => s.AuthenticateAsync("testuser", "secret123"))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.CheckUserAsync("testuser", "secret123");

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        Assert.Equal(5, response.Data.GetProperty("Id").GetInt32());
        Assert.Equal("testuser", response.Data.GetProperty("Username").GetString());
        Assert.Equal("secret123", response.Data.GetProperty("PasswordFromDb").GetString());
        Assert.Equal("secret123", response.Data.GetProperty("InputPassword").GetString());
        Assert.Equal("Test", response.Data.GetProperty("FirstName").GetString());
        Assert.Equal("User", response.Data.GetProperty("LastName").GetString());
    }

    [Fact]
    public async Task CheckUserAsync_InvalidCredentials_ReturnsError()
    {
        // Arrange
        _mockAuthService.Setup(s => s.AuthenticateAsync("wrong", "wrong"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.CheckUserAsync("wrong", "wrong");

        // Assert
        var errorResponse = ParseErrorResponse(result, 400);
        Assert.False(errorResponse.Success);
        Assert.Equal("User not found or password incorrect", errorResponse.Error);
    }

    [Fact]
    public async Task CheckUserAsync_EmptyUsername_ReturnsError()
    {
        // Arrange
        _mockAuthService.Setup(s => s.AuthenticateAsync("", "pass"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.CheckUserAsync("", "pass");

        // Assert
        var errorResponse = ParseErrorResponse(result, 400);
        Assert.False(errorResponse.Success);
        Assert.Equal("User not found or password incorrect", errorResponse.Error);
    }

    [Fact]
    public async Task CheckUserAsync_EmptyPassword_ReturnsError()
    {
        // Arrange
        _mockAuthService.Setup(s => s.AuthenticateAsync("user", ""))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.CheckUserAsync("user", "");

        // Assert
        var errorResponse = ParseErrorResponse(result, 400);
        Assert.False(errorResponse.Success);
        Assert.Equal("User not found or password incorrect", errorResponse.Error);
    }
    
    [Fact]
    public async Task CheckUserAsync_UserWithoutPerson_ReturnsError()
    {
        // Arrange
        var user = new User
        {
            Id = 1
            // Person را مقداردهی نمی‌کنیم - پیش‌فرض null است
        };
        // از آنجایی که Person nullable نیست، باید از عملگر null-forgiving استفاده کنیم
        // یا بهتر: این سناریو واقعاً در برنامه رخ نمی‌دهد چون User همیشه با Person ساخته می‌شود
        
        _mockAuthService.Setup(s => s.AuthenticateAsync("noperson", "pass"))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.CheckUserAsync("noperson", "pass");

        // Assert
        var response = ParseSuccessResponse<JsonElement>(result);
        Assert.True(response.Success);
        Assert.Equal(1, response.Data.GetProperty("Id").GetInt32());
        Assert.Equal("pass", response.Data.GetProperty("InputPassword").GetString());
    }
    #endregion
}