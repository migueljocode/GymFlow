namespace GymFlow.Web.Pages;

public class LogoutModel : PageModel
{
    private readonly ApiClient _apiClient;

    public LogoutModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult OnGet()
    {
        _apiClient.Logout();
        HttpContext.Session.Clear();
        return RedirectToPage("/Login");
    }
}