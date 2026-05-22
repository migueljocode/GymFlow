namespace GymFlow.Tests.Api.Controllers;

public class AuthControllerTest : ControllerTestFixture
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ICoachRepository> _mockCoachRepository;
    private readonly Mock<IPersonRepository> _mockPersonRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly AuthController _controller;

    public AuthControllerTest()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockCoachRepository = new Mock<ICoachRepository>();
        _mockPersonRepository = new Mock<IPersonRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        
        _controller = CreateController<AuthController>(
            _mockAuthService.Object,
            _mockCoachRepository.Object,
            _mockPersonRepository.Object,
            _mockUserRepository.Object);
    }

    // ========== Helper ==========
    private ApiResponse<T> DeserializeApiResponse<T>(IActionResult result)
    {
        // به جای Assert.IsType<ObjectResult>، از IsAssignableFrom استفاده کنید
        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        var json = JsonSerializer.Serialize(objectResult.Value, JsonOptions);
        return Deserialize<ApiResponse<T>>(json);
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
        var response = DeserializeApiResponse<JsonElement>(okResult);

        Assert.True(response.Success);
        Assert.Equal("Login successful", response.Message);
        
        var data = response.Data;
        Assert.Equal(1, data.GetProperty("Id").GetInt32());
        Assert.Equal("coach", data.GetProperty("Username").GetString());
        Assert.Equal("Coach", data.GetProperty("Role").GetString());
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
        var response = DeserializeApiResponse<JsonElement>(okResult);

        Assert.True(response.Success);
        Assert.Equal("Login successful", response.Message);
        
        var data = response.Data;
        Assert.Equal(2, data.GetProperty("Id").GetInt32());
        Assert.Equal("member", data.GetProperty("Username").GetString());
        Assert.Equal("Member", data.GetProperty("Role").GetString());
    }

    [Fact]
    public async Task LoginAsync_WithEmptyUsername_ShouldReturnError()
    {
        var request = new LoginRequest { Username = "", Password = "pass" };

        var result = await _controller.LoginAsync(request);
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var response = DeserializeApiResponse<JsonElement>(badRequestResult);

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
        var response = DeserializeApiResponse<JsonElement>(badRequestResult);

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
        var response = DeserializeApiResponse<JsonElement>(badRequestResult);

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
        var response = DeserializeApiResponse<JsonElement>(okResult);

        Assert.True(response.Success);
        
        var data = response.Data;
        Assert.Equal(5, data.GetProperty("Id").GetInt32());
        Assert.Equal("testuser", data.GetProperty("Username").GetString());
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

// کلاس ApiResponse باید خارج از کلاس AuthControllerTest باشد
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Error { get; set; }
    public List<string>? Errors { get; set; }
}