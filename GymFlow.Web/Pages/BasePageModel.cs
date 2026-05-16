using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymFlow.Web.Pages;

public abstract class BasePageModel : PageModel
{
    public bool IsMember => HttpContext.Session.GetString("UserRole") == "Member";
    public bool IsCoach => HttpContext.Session.GetString("UserRole") == "Coach";

    protected IActionResult? RedirectIfNotMember()
    {
        if (!IsMember) return RedirectToPage("/Login");
        return null;
    }

    protected IActionResult? RedirectIfNotCoach()
    {
        if (!IsCoach) return RedirectToPage("/Login");
        return null;
    }
}