namespace GymFlow.Web.Pages;

public class ProfileModel : BasePageModel
{
    private readonly ApiClient _apiClient;

    public ProfileModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public bool IsUserCoach { get; set; } = false;

    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    public string? Email { get; set; }

    [BindProperty]
    public string? Phone { get; set; }

    [BindProperty]
    public string Specialization { get; set; } = string.Empty;

    [BindProperty]
    public int YearsOfExperience { get; set; }

    [BindProperty]
    public Goal Goal { get; set; }

    [BindProperty]
    public float? Weight { get; set; }

    [BindProperty]
    public float? Height { get; set; }

    [BindProperty]
    public BodyType? BodyType { get; set; }

    [BindProperty]
    public int? EstimatedCaloriesIntake { get; set; }

    [BindProperty]
    public bool IsCompetitive { get; set; }

    [BindProperty]
    public int? SelectedCoachId { get; set; }

    public string Username { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public List<CoachListItemResponse> CoachesList { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole))
            return RedirectToPage("/Login");

        IsUserCoach = userRole == "Coach";

        if (IsUserCoach)
        {
            await LoadCoachProfileAsync();
        }
        else
        {
            await LoadMemberProfileAsync();
            CoachesList = await _apiClient.GetAsync<List<CoachListItemResponse>>("api/coaches/list") ?? new();
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole))
            return RedirectToPage("/Login");

        IsUserCoach = userRole == "Coach";

        if (IsUserCoach)
        {
            ModelState.Remove("Goal");
            ModelState.Remove("Weight");
            ModelState.Remove("Height");
            ModelState.Remove("BodyType");
            ModelState.Remove("EstimatedCaloriesIntake");
            ModelState.Remove("IsCompetitive");
            ModelState.Remove("SelectedCoachId");
        }
        else
        {
            ModelState.Remove("Specialization");
            ModelState.Remove("YearsOfExperience");
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["ErrorMessage"] = $"لطفاً اطلاعات را به درستی وارد کنید: {string.Join(" | ", errors)}";
            return Page();
        }

        bool success;
        if (IsUserCoach)
        {
            var updateData = new UpdateCoachProfileRequest
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Phone = Phone,
                Specialization = Specialization,
                YearsOfExperience = YearsOfExperience
            };
            success = await _apiClient.PutAsync("api/coaches/me", updateData);
            if (success)
            {
                TempData["Message"] = "اطلاعات پروفایل مربی با موفقیت به‌روز شد.";
                await LoadCoachProfileAsync();
            }
            else
            {
                TempData["ErrorMessage"] = "خطا در به‌روزرسانی پروفایل مربی.";
            }
        }
        else
        {
            var userId = GetCurrentUserIdFromSession();
            if (userId == null) return RedirectToPage("/Login");

            var updateData = new UpdateUserProfileRequest
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Phone = Phone,
                Goal = Goal,
                Weight = Weight,
                Height = Height,
                BodyType = BodyType,
                EstimatedCaloriesIntake = EstimatedCaloriesIntake,
                IsCompetitive = IsCompetitive,
                CoachId = SelectedCoachId
            };
            success = await _apiClient.PutAsync($"api/users/{userId}", updateData);
            
            if (success)
            {
                TempData["Message"] = "اطلاعات پروفایل با موفقیت به‌روز شد.";
                await LoadMemberProfileAsync();
            }
            else
            {
                TempData["ErrorMessage"] = "خطا در به‌روزرسانی پروفایل.";
            }
        }

        return Page();
    }

    private async Task LoadCoachProfileAsync()
    {
        var profile = await _apiClient.GetAsync<CoachProfileResponse>("api/coaches/me");
        if (profile != null)
        {
            FirstName = profile.FirstName;
            LastName = profile.LastName;
            Email = profile.Email;
            Phone = profile.Phone;
            Specialization = profile.Specialization;
            YearsOfExperience = profile.YearsOfExperience;
            Username = profile.Username;
            CreatedAt = profile.CreatedAt.ToString("yyyy/MM/dd");
        }
        else
        {
            TempData["ErrorMessage"] = "خطا در دریافت اطلاعات پروفایل مربی.";
        }
    }

    private async Task LoadMemberProfileAsync()
    {
        var userId = GetCurrentUserIdFromSession();
        if (userId == null) return;

        var profile = await _apiClient.GetAsync<UserProfileResponse>($"api/users/{userId}");
        if (profile != null)
        {
            FirstName = profile.FirstName;
            LastName = profile.LastName;
            Email = profile.Email;
            Phone = profile.Phone;
            Goal = profile.Goal;
            Weight = profile.Weight;
            Height = profile.Height;
            BodyType = profile.BodyType;
            EstimatedCaloriesIntake = profile.EstimatedCaloriesIntake;
            IsCompetitive = profile.IsCompetitive;
            SelectedCoachId = profile.CoachId;
            Username = profile.Username;
            CreatedAt = profile.CreatedAt.ToString("yyyy/MM/dd");
        }
        else
        {
            TempData["ErrorMessage"] = "خطا در دریافت اطلاعات پروفایل کاربر.";
        }
    }

    private int? GetCurrentUserIdFromSession()
    {
        return int.TryParse(HttpContext.Session.GetString("UserId"), out var userId) ? userId : null;
    }
}