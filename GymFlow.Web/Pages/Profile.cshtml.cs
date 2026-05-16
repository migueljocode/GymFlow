using Microsoft.AspNetCore.Mvc;

namespace GymFlow.Web.Pages;

public class ProfileModel : BasePageModel
{
    public IActionResult OnGet()
    {
        // هر دو نقش می‌توانند پروفایل ببینند
        if (!IsMember && !IsCoach) return RedirectToPage("/Login");
        return Page();
    }
}