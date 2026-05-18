namespace GymFlow.Web.Pages;

public class LoginModel : PageModel
{
    private readonly ApiClient _apiClient;

    public LoginModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            TempData["ErrorMessage"] = "لطفاً نام کاربری و رمز عبور را وارد کنید";
            return Page();
        }

        var (success, userId) = await _apiClient.LoginAsync(Username, Password);

        if (success)
        {
            // تشخیص نقش واقعی کاربر
            string userRole = "Member";
            
            // روش 1: بررسی با نام کاربری (سریع)
            if (Username == "coach")
            {
                userRole = "Coach";
            }
            else
            {
                // روش 2: بررسی با API (برای کاربرانی که بعداً ثبت‌نام می‌کنند)
                try
                {
                    // سعی می‌کنیم اطلاعات مربی را بگیریم
                    var coachData = await _apiClient.GetAsync<object>($"api/coaches/user/{userId}");
                    if (coachData != null)
                    {
                        userRole = "Coach";
                    }
                }
                catch
                {
                    userRole = "Member";
                }
            }
            
            HttpContext.Session.SetString("Username", Username);
            HttpContext.Session.SetString("UserRole", userRole);
            HttpContext.Session.SetString("UserId", userId.ToString());
            
            Console.WriteLine($"[DEBUG] Login - User: {Username}, UserId: {userId}, Role: {userRole}");
            
            return RedirectToPage("/Index");
        }

        TempData["ErrorMessage"] = "نام کاربری یا رمز عبور اشتباه است";
        return Page();
    }
}