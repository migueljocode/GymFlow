using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymFlow.Web.Pages;

public abstract class BasePageModel : PageModel
{
    protected IActionResult? RedirectIfNotLoggedIn()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
        {
            return RedirectToPage("/Login");
        }
        return null;
    }
}