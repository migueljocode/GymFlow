using Microsoft.AspNetCore.Mvc;

namespace GymFlow.Web.Pages.Coach;

public class ClientsModel : BasePageModel
{
    public IActionResult OnGet()
    {
        var redirect = RedirectIfNotCoach();
        if (redirect != null) return redirect;
        return Page();
    }
}