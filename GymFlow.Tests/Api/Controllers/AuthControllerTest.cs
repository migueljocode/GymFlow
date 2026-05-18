namespace GymFlow.Tests.Api.Controllers;

public class AuthControllerTest : ControllerTestFixture
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    // کلاس کمکی برای دیسریالایز پاسخ استاندارد
    private class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public JsonElement? Data { get; set; }
        public string? Error { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public AuthControllerTest()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = CreateController<AuthController>(_mockAuthService.Object);
    }

    // ========== Helper ==========
    private ApiResponse DeserializeApiResponse(IActionResult result)
    {
        var json = JsonSerializer.Serialize(((ObjectResult)result).Value);
        return Deserialize<ApiResponse>(json);
    }

    // ========== LoginAsync Tests ==========

    [Fact]
    public async Task LoginAsync_WithValidCoachCredentials_ShouldReturnSuccess()
    {
        var request = new LoginRequest { Username = "coach", Password = "coach123" };
        var user = new User
        {
            Id = 1,
            Person = new Person
            {
                Username = "coach",
                FirstName = "Master",
                LastName = "Coach",
                Email = "coach@gymflow.com"
            }
        };
        _mockAuthService.Setup(s => s.AuthenticateAsync("coach", "coach123"))
            .ReturnsAsync(user);

        var result = await _controller.LoginAsync(request);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = DeserializeApiResponse(okResult);

        Assert.True(response.Success);
        Assert.Equal("Login successful", response.Message);
        Assert.NotNull(response.Data);
        // توجه: نام خواص در JSON با حرف بزرگ (PascalCase) هستند
        Assert.Equal(1, response.Data.Value.GetProperty("Id").GetInt32());
        Assert.Equal("coach", response.Data.Value.GetProperty("Username").GetString());
        Assert.Equal("Coach", response.Data.Value.GetProperty("Role").GetString());
    }

    [Fact]
    public async Task LoginAsync_WithValidMemberCredentials_ShouldReturnSuccess()
    {
        var request = new LoginRequest { Username = "member", Password = "member123" };
        var user = new User
        {
            Id = 2,
            Person = new Person
            {
                Username = "member",
                FirstName = "John",
                LastName = "Doe",
                Email = "member@gymflow.com"
            }
        };
        _mockAuthService.Setup(s => s.AuthenticateAsync("member", "member123"))
            .ReturnsAsync(user);

        var result = await _controller.LoginAsync(request);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = DeserializeApiResponse(okResult);

        Assert.True(response.Success);
        Assert.Equal("Login successful", response.Message);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Value.GetProperty("Id").GetInt32());
        Assert.Equal("member", response.Data.Value.GetProperty("Username").GetString());
        Assert.Equal("Member", response.Data.Value.GetProperty("Role").GetString());
    }

    [Fact]
    public async Task LoginAsync_WithEmptyUsername_ShouldReturnError()
    {
        var request = new LoginRequest { Username = "", Password = "pass" };

        var result = await _controller.LoginAsync(request);
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var response = DeserializeApiResponse(badRequestResult);

        Assert.False(response.Success);
        Assert.Equal("Username and password are required", response.Error);
    }

    [Fact]
    public async Task LoginAsync_WithEmptyPassword_ShouldReturnError()
    {
        var request = new LoginRequest { Username = "user", Password = "" };

        var result = await _controller.LoginAsync(request);
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var response = DeserializeApiResponse(badRequestResult);

        Assert.False(response.Success);
        Assert.Equal("Username and password are required", response.Error);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldReturnError()
    {
        var request = new LoginRequest { Username = "wrong", Password = "wrong" };
        _mockAuthService.Setup(s => s.AuthenticateAsync("wrong", "wrong"))
            .ReturnsAsync((User?)null);

        var result = await _controller.LoginAsync(request);
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var response = DeserializeApiResponse(badRequestResult);

        Assert.False(response.Success);
        Assert.Equal("Invalid username or password", response.Error);
    }

    // ========== GetCurrentUser Tests ==========

    [Fact]
    public void GetCurrentUser_WhenUserIdExistsInHttpContext_ShouldReturnSuccess()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = 5;
        httpContext.Items["Username"] = "testuser";
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = _controller.GetCurrentUser();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = DeserializeApiResponse(okResult);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(5, response.Data.Value.GetProperty("Id").GetInt32());
        Assert.Equal("testuser", response.Data.Value.GetProperty("Username").GetString());
    }

    [Fact]
    public void GetCurrentUser_WhenUserIdMissing_ShouldReturnUnauthorized()
    {
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = _controller.GetCurrentUser();
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void GetCurrentUser_WhenUserIdIsNotInt_ShouldReturnUnauthorized()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = "not an int";
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = _controller.GetCurrentUser();
        Assert.IsType<UnauthorizedResult>(result);
    }
}