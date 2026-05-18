namespace GymFlow.Tests.Api.Controllers.Base;

public class ApiControllerBaseTest : ControllerTestFixture
{
    // کنترلر مشتق شده برای دسترسی به متدهای protected
    private class TestController : ApiControllerBase
    {
        public void AddModelError(string key, string errorMessage)
        {
            ModelState.AddModelError(key, errorMessage);
        }

        // بازنویسی متدهای protected به صورت public
        public new IActionResult Success<T>(T? data, string? message = null)
            => base.Success(data, message);

        public new IActionResult SuccessWithPagination<T>(T? data, int totalCount, int page, int pageSize, string? message = null)
            => base.SuccessWithPagination(data, totalCount, page, pageSize, message);

        public new IActionResult CreatedResponse<T>(T data, string? message = null)
            => base.CreatedResponse(data, message);

        public new IActionResult Error(string message, int statusCode = 400, List<string>? errors = null)
            => base.Error(message, statusCode, errors);

        public new IActionResult NotFoundResponse(string entityName, object? identifier = null)
            => base.NotFoundResponse(entityName, identifier);

        public new IActionResult ValidationErrorResponse()
            => base.ValidationErrorResponse();

        public new int GetCurrentUserId()
            => base.GetCurrentUserId();

        public new bool IsAuthenticated()
            => base.IsAuthenticated();
    }

    private TestController CreateTestController()
    {
        var controller = CreateController<TestController>();
        // همچنین می‌توانید موارد اضافی مانند Context را تنظیم کنید
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
            ActionDescriptor = new ControllerActionDescriptor()
        };
        return controller;
    }

    // کلاس پاسخ برای دیسریالایز (در صورت نیاز در Fixture هم می‌توان تعریف کرد)
    private class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public JsonElement? Data { get; set; }
        public DateTime Timestamp { get; set; }
        public JsonElement? Pagination { get; set; }
        public string? Error { get; set; }
        public List<string>? Errors { get; set; }
    }

    // ========== Success Tests ==========

    [Fact]
    public void Success_ShouldReturnOkWithCorrectStructure()
    {
        var controller = CreateTestController();
        var data = new { Id = 1, Name = "Test" };
        var message = "Success message";

        var result = controller.Success(data, message);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Deserialize<ApiResponse>(JsonSerializer.Serialize(okResult.Value));

        Assert.True(response.Success);
        Assert.Equal(message, response.Message);
        Assert.NotNull(response.Data);
        Assert.Equal(1, response.Data!.Value.GetProperty("Id").GetInt32());
        Assert.Equal("Test", response.Data.Value.GetProperty("Name").GetString());
        Assert.True(response.Timestamp != default);
    }

    [Fact]
    public void Success_WithoutMessage_ShouldReturnOkWithNullMessage()
    {
        var controller = CreateTestController();
        var data = "test";

        var result = controller.Success(data);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Deserialize<ApiResponse>(JsonSerializer.Serialize(okResult.Value));

        Assert.True(response.Success);
        Assert.Null(response.Message);
        Assert.Equal(data, response.Data!.Value.GetString());
    }

    [Fact]
    public void Success_WithNullData_ShouldReturnOkWithNullData()
    {
        var controller = CreateTestController();

        var result = controller.Success<object>(null);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Deserialize<ApiResponse>(JsonSerializer.Serialize(okResult.Value));

        Assert.True(response.Success);
        Assert.Null(response.Data);
    }

    // ========== SuccessWithPagination Tests ==========

    [Fact]
    public void SuccessWithPagination_ShouldReturnOkWithPaginationInfo()
    {
        var controller = CreateTestController();
        var data = new[] { 1, 2, 3 };
        int totalCount = 10;
        int page = 2;
        int pageSize = 3;
        string message = "Paginated result";

        var result = controller.SuccessWithPagination(data, totalCount, page, pageSize, message);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Deserialize<ApiResponse>(JsonSerializer.Serialize(okResult.Value));

        Assert.True(response.Success);
        Assert.Equal(message, response.Message);
        Assert.NotNull(response.Data);
        Assert.NotNull(response.Pagination);
        var pagination = response.Pagination.Value;
        Assert.Equal(totalCount, pagination.GetProperty("totalCount").GetInt32());
        Assert.Equal(page, pagination.GetProperty("page").GetInt32());
        Assert.Equal(pageSize, pagination.GetProperty("pageSize").GetInt32());
        Assert.Equal((int)Math.Ceiling((double)totalCount / pageSize), pagination.GetProperty("totalPages").GetInt32());
    }

    // ========== CreatedResponse Tests ==========

    [Fact]
    public void CreatedResponse_ShouldReturnStatusCode201WithCorrectStructure()
    {
        var controller = CreateTestController();
        var data = new { Id = 1 };
        var message = "Created successfully";

        var result = controller.CreatedResponse(data, message);
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, statusCodeResult.StatusCode);
        var response = Deserialize<ApiResponse>(JsonSerializer.Serialize(statusCodeResult.Value));

        Assert.True(response.Success);
        Assert.Equal(message, response.Message);
        Assert.NotNull(response.Data);
        Assert.Equal(1, response.Data!.Value.GetProperty("Id").GetInt32());
    }

    [Fact]
    public void CreatedResponse_WithoutMessage_ShouldUseDefaultMessage()
    {
        var controller = CreateTestController();
        var data = "resource";

        var result = controller.CreatedResponse(data);
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        var response = Deserialize<ApiResponse>(JsonSerializer.Serialize(statusCodeResult.Value));

        Assert.Equal("Resource created successfully", response.Message);
    }

    // ========== Error Tests ==========

    [Fact]
    public void Error_ShouldReturnStatusCodeWithErrorDetails()
    {
        var controller = CreateTestController();
        var errorMessage = "Something went wrong";
        int statusCode = 400;
        var errors = new List<string> { "Field1 is required", "Field2 too long" };

        var result = controller.Error(errorMessage, statusCode, errors);
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(statusCode, statusCodeResult.StatusCode);
        var response = Deserialize<ApiResponse>(JsonSerializer.Serialize(statusCodeResult.Value));

        Assert.False(response.Success);
        Assert.Equal(errorMessage, response.Error);
        Assert.NotNull(response.Errors);
        Assert.Equal(errors.Count, response.Errors.Count);
        Assert.Equal(errors[0], response.Errors[0]);
        Assert.Equal(errors[1], response.Errors[1]);
    }

    [Fact]
    public void Error_WithoutErrors_ShouldReturnNullErrors()
    {
        var controller = CreateTestController();

        var result = controller.Error("Error", 400, null);
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        var response = Deserialize<ApiResponse>(JsonSerializer.Serialize(statusCodeResult.Value));

        Assert.Null(response.Errors);
    }

    // ========== NotFoundResponse Tests ==========

    [Fact]
    public void NotFoundResponse_WithIdentifier_ShouldReturnNotFoundWithEntityNameAndIdentifier()
    {
        var controller = CreateTestController();
        string entityName = "User";
        object identifier = 5;

        var result = controller.NotFoundResponse(entityName, identifier);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Deserialize<ApiResponse>(JsonSerializer.Serialize(notFoundResult.Value));

        Assert.False(response.Success);
        Assert.Equal($"{entityName} with identifier '{identifier}' not found", response.Error);
    }

    [Fact]
    public void NotFoundResponse_WithoutIdentifier_ShouldReturnNotFoundWithEntityNameOnly()
    {
        var controller = CreateTestController();
        string entityName = "Product";

        var result = controller.NotFoundResponse(entityName);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Deserialize<ApiResponse>(JsonSerializer.Serialize(notFoundResult.Value));

        Assert.Equal($"{entityName} not found", response.Error);
    }

    // ========== ValidationErrorResponse Tests ==========

    [Fact]
    public void ValidationErrorResponse_WhenModelStateHasErrors_ShouldReturnBadRequestWithErrors()
    {
        var controller = CreateTestController();
        controller.AddModelError("Name", "Name is required");
        controller.AddModelError("Age", "Age must be positive");

        var result = controller.ValidationErrorResponse();
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var response = Deserialize<ApiResponse>(JsonSerializer.Serialize(badRequestResult.Value));

        Assert.False(response.Success);
        Assert.Equal("Validation failed", response.Error);
        Assert.NotNull(response.Errors);
        Assert.Equal(2, response.Errors.Count);
    }

    [Fact]
    public void ValidationErrorResponse_WhenModelStateHasNoErrors_ShouldReturnBadRequestWithEmptyErrors()
    {
        var controller = CreateTestController();

        var result = controller.ValidationErrorResponse();
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        var response = Deserialize<ApiResponse>(JsonSerializer.Serialize(badRequestResult.Value));

        Assert.NotNull(response.Errors);
        Assert.Empty(response.Errors);
    }

    // ========== GetCurrentUserId / IsAuthenticated ==========

    [Fact]
    public void GetCurrentUserId_ShouldReturnOne()
    {
        var controller = CreateTestController();
        var userId = controller.GetCurrentUserId();
        Assert.Equal(1, userId);
    }

    [Fact]
    public void IsAuthenticated_ShouldReturnTrue()
    {
        var controller = CreateTestController();
        var isAuth = controller.IsAuthenticated();
        Assert.True(isAuth);
    }
}