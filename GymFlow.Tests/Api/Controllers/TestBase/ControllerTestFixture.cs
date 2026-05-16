using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace GymFlow.Tests.Api.Controllers.TestBase;

/// <summary>
/// Fixture پایه برای تست کنترلرهای API
/// </summary>
public abstract class ControllerTestFixture
{
    // تنظیمات JsonSerializer برای نادیده گرفتن حروف بزرگ/کوچک
    protected static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// کلاس استاندارد پاسخ API
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Error { get; set; }
        public List<string>? Errors { get; set; }
        public JsonElement? Pagination { get; set; }
    }

    /// <summary>
    /// ایجاد یک کنترلر با وابستگی‌های تزریق شده
    /// </summary>
    protected TController CreateController<TController>(params object[] constructorArgs) where TController : ControllerBase
    {
        var controller = (TController)Activator.CreateInstance(typeof(TController), constructorArgs)!;
        ConfigureControllerContext(controller);
        return controller;
    }

    /// <summary>
    /// تنظیمات پایه Context برای کنترلر
    /// </summary>
    protected virtual void ConfigureControllerContext(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
            ActionDescriptor = new ControllerActionDescriptor()
        };
    }

    /// <summary>
    /// دیسریالایز کردن JSON به نوع مشخص
    /// </summary>
    protected T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, JsonOptions)!;

    /// <summary>
    /// تبدیل IActionResult موفق (Status 200) به ApiResponse
    /// </summary>
    protected ApiResponse<T> ParseSuccessResponse<T>(IActionResult result)
    {
        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = JsonSerializer.Serialize(okResult.Value, JsonOptions);
        return Deserialize<ApiResponse<T>>(json);
    }

    /// <summary>
    /// تبدیل IActionResult موفق (Status 201 Created)
    /// </summary>
    protected ApiResponse<T> ParseCreatedResponse<T>(IActionResult result)
    {
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
        var json = JsonSerializer.Serialize(objectResult.Value, JsonOptions);
        return Deserialize<ApiResponse<T>>(json);
    }

    /// <summary>
    /// تبدیل IActionResult خطا (BadRequest, NotFound, ...) به ApiResponse
    /// </summary>
    protected ApiResponse<object> ParseErrorResponse(IActionResult result, int expectedStatusCode)
    {
        // استفاده از IsAssignableFrom برای پذیرش NotFoundObjectResult و سایر مشتقات
        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
        var json = JsonSerializer.Serialize(objectResult.Value, JsonOptions);
        return Deserialize<ApiResponse<object>>(json);
    }
}