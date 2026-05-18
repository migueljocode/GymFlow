#nullable disable

namespace GymFlow.Tests.Web.Pages.TestBase;

/// <summary>
/// پیاده‌سازی ساده از ISession برای تست
/// </summary>
public class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new();

    public bool IsAvailable => true;
    public string Id => Guid.NewGuid().ToString();
    public IEnumerable<string> Keys => _store.Keys;

    public void Clear() => _store.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Remove(string key) => _store.Remove(key);
    public void Set(string key, byte[] value) => _store[key] = value;
    public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
}

/// <summary>
/// Fixture پایه برای تست PageModelهای Razor Pages
/// </summary>
public abstract class PageModelTestFixture
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected TPageModel CreatePageModel<TPageModel>(params object[] constructorArgs) where TPageModel : PageModel
    {
        var pageModel = (TPageModel)Activator.CreateInstance(typeof(TPageModel), constructorArgs);
        ConfigurePageModel(pageModel);
        return pageModel;
    }

    protected virtual void ConfigurePageModel(PageModel pageModel)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();  // مقداردهی مستقیم سشن

        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        pageModel.PageContext = pageContext;
        pageModel.Url = new UrlHelper(actionContext);
        pageModel.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    protected void AddModelError(PageModel pageModel, string key, string errorMessage)
    {
        pageModel.ModelState.AddModelError(key, errorMessage);
    }

    protected PageResult AssertPageResult(IActionResult result)
    {
        return Assert.IsType<PageResult>(result);
    }

    protected RedirectToPageResult AssertRedirectToPage(IActionResult result, string expectedPageName)
    {
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal(expectedPageName, redirectResult.PageName);
        return redirectResult;
    }

    protected RedirectResult AssertRedirect(IActionResult result, string expectedUrl)
    {
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal(expectedUrl, redirectResult.Url);
        return redirectResult;
    }

    protected NotFoundResult AssertNotFound(IActionResult result)
    {
        return Assert.IsType<NotFoundResult>(result);
    }

    protected ObjectResult AssertBadRequest(IActionResult result)
    {
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
        return objectResult;
    }

    protected void SetSessionValue(PageModel pageModel, string key, string value)
    {
        pageModel.HttpContext.Session.SetString(key, value);
    }

    protected string GetSessionValue(PageModel pageModel, string key)
    {
        return pageModel.HttpContext.Session.GetString(key);
    }

    protected void SetAuthenticatedUser(PageModel pageModel, int userId, string username, string role = "Member")
    {
        SetSessionValue(pageModel, "UserId", userId.ToString());
        SetSessionValue(pageModel, "Username", username);
        SetSessionValue(pageModel, "UserRole", role);
    }
}