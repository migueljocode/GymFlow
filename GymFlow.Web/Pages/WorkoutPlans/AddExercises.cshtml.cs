using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using GymFlow.Web.Services;
using Microsoft.Extensions.Primitives;

namespace GymFlow.Web.Pages.WorkoutPlans;

public class AddExercisesModel : BasePageModel  // ← تغییر ارث‌بری
{
    private readonly ApiClient _apiClient;
    
    public AddExercisesModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    [BindProperty]
    public int WorkoutDayId { get; set; }
    
    [BindProperty]
    public int WorkoutPlanId { get; set; }
    
    [BindProperty]
    public string DayOfWeek { get; set; } = string.Empty;
    
    [BindProperty]
    public int ClientId { get; set; }  // ← اضافه شد (id مشتری جاری)
    
    [BindProperty]
    public int TargetMuscles { get; set; }
    
    [BindProperty]
    public int Intensity { get; set; }
    
    [BindProperty]
    public int DurationMinutes { get; set; }
    
    [BindProperty]
    public string? Notes { get; set; }
    
    [BindProperty]
    public int NewExerciseId { get; set; }
    
    [BindProperty]
    public int NewSets { get; set; } = 3;
    
    [BindProperty]
    public string NewReps { get; set; } = "10,10,8";
    
    [BindProperty]
    public int NewRestSeconds { get; set; } = 60;
    
    [BindProperty]
    public string? NewNotes { get; set; }
    
    public List<SelectListItem> ExerciseList { get; set; } = new();
    public List<WorkoutExerciseItem> ExistingExercises { get; set; } = new();
    
    public async Task<IActionResult> OnGetAsync(int workoutDayId, int workoutPlanId, string dayOfWeek)
    {
        var userIdParam = Request.Query["userId"].ToString();
        Console.WriteLine($"[DEBUG] userId from Request.Query: {userIdParam}");

        if (!IsCoach)
            return RedirectToPage("/Login");

        if (!int.TryParse(userIdParam, out var userId) || userId == 0)
            return RedirectToPage("/Coach/Clients");

        WorkoutDayId = workoutDayId;
        WorkoutPlanId = workoutPlanId;
        DayOfWeek = dayOfWeek;
        ClientId = userId;

        Console.WriteLine($"[DEBUG] ClientId set to: {ClientId}");

        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var action = Request.Form["action"].ToString();

        if (action == "finish")
        {
            await SaveAllChangesAsync();

            foreach (var key in Request.Form.Keys)
            {
                if (key != null && key.StartsWith("ExerciseSets["))
                {
                    var startIndex = "ExerciseSets[".Length;
                    var endIndex = key.IndexOf(']', startIndex);
                    if (endIndex > startIndex)
                    {
                        var exerciseIdStr = key.Substring(startIndex, endIndex - startIndex);
                        if (int.TryParse(exerciseIdStr, out var exerciseId))
                        {
                            if (int.TryParse(Request.Form[key], out var sets))
                            {
                                var reps = Request.Form[$"ExerciseReps[{exerciseId}]"].ToString() ?? string.Empty;
                                if (int.TryParse(Request.Form[$"ExerciseRest[{exerciseId}]"], out var restSeconds))
                                {
                                    await UpdateExerciseAsync(exerciseId, sets, reps, restSeconds);
                                }
                            }
                        }
                    }
                }
            }
            
            Console.WriteLine($"[DEBUG] Redirecting: WorkoutPlanId={WorkoutPlanId}, ClientId={ClientId}");
            return RedirectToPage("/WorkoutPlans/Details", new { id = WorkoutPlanId, userId = ClientId });
        }
        else if (action == "add")
        {
            await AddExerciseAsync();
            TempData["Message"] = "حرکت با موفقیت اضافه شد!";
            return RedirectToPage(new { workoutDayId = WorkoutDayId, workoutPlanId = WorkoutPlanId, dayOfWeek = DayOfWeek, userId = ClientId });
        }
        else if (!string.IsNullOrEmpty(action) && action.StartsWith("delete_"))
        {
            var parts = action.Split('_');
            if (parts.Length > 1 && int.TryParse(parts[1], out var exerciseId))
            {
                await DeleteExerciseAsync(exerciseId);
                TempData["Message"] = "حرکت با موفقیت حذف شد!";
            }
            return RedirectToPage(new { workoutDayId = WorkoutDayId, workoutPlanId = WorkoutPlanId, dayOfWeek = DayOfWeek, userId = ClientId });
        }

        return RedirectToPage(new { workoutDayId = WorkoutDayId, workoutPlanId = WorkoutPlanId, dayOfWeek = DayOfWeek, userId = ClientId });
    }
    
    private async Task SaveAllChangesAsync()
    {
        var updateRequest = new
        {
            TargetMuscles = TargetMuscles,
            Intensity = Intensity,
            DurationMinutes = DurationMinutes,
            Notes = Notes ?? ""
        };
        await _apiClient.PutAsync($"api/workoutdays/{WorkoutDayId}", updateRequest);
    }
    
    private async Task AddExerciseAsync()
    {
        var request = new
        {
            WorkoutDayId = WorkoutDayId,
            ExerciseId = NewExerciseId,
            Sets = NewSets,
            Reps = NewReps,
            RestSeconds = NewRestSeconds,
            Notes = NewNotes ?? ""
        };
        await _apiClient.PostAsync<object>("api/workoutdayexercises", request);
    }
    
    private async Task UpdateExerciseAsync(int exerciseId, int sets, string reps, int restSeconds)
    {
        var request = new
        {
            Sets = sets,
            Reps = reps,
            RestSeconds = restSeconds
        };
        await _apiClient.PutAsync($"api/workoutdayexercises/{exerciseId}", request);
    }
    
    private async Task DeleteExerciseAsync(int exerciseId)
    {
        await _apiClient.DeleteAsync($"api/workoutdayexercises/{exerciseId}");
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            var exercises = await _apiClient.GetAsync<List<ExerciseItem>>("api/exercises");
            if (exercises != null)
            {
                ExerciseList = exercises.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Name} - {e.MuscleGroup}"
                }).ToList();
            }
            
            var plan = await _apiClient.GetAsync<WorkoutPlanDetails>($"api/workoutplans/{WorkoutPlanId}/details");
            if (plan != null && plan.WorkoutDays != null)
            {
                var targetDay = plan.WorkoutDays.FirstOrDefault(d => d.Id == WorkoutDayId);
                if (targetDay != null)
                {
                    TargetMuscles = targetDay.TargetMuscles;
                    Intensity = targetDay.Intensity;
                    DurationMinutes = targetDay.DurationMinutes;
                    Notes = targetDay.Notes;
                    
                    if (targetDay.Exercises != null && targetDay.Exercises.Any())
                    {
                        ExistingExercises = targetDay.Exercises.Select(e => new WorkoutExerciseItem
                        {
                            Id = e.Id,
                            ExerciseName = e.ExerciseName,
                            Sets = e.Sets,
                            Reps = e.Reps,
                            RestSeconds = e.RestSeconds,
                            Notes = e.Notes
                        }).ToList();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading data: {ex.Message}");
        }
    }
}

public class ExerciseItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
}

public class WorkoutPlanDetails
{
    public int Id { get; set; }
    public List<WorkoutDayDetail> WorkoutDays { get; set; } = new();
}

public class WorkoutDayDetail
{
    public int Id { get; set; }
    public int TargetMuscles { get; set; }
    public int Intensity { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public List<ExerciseInDay> Exercises { get; set; } = new();
}

public class ExerciseInDay
{
    public int Id { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Sets { get; set; }
    public string Reps { get; set; } = string.Empty;
    public int RestSeconds { get; set; }
    public string? Notes { get; set; }
}

public class WorkoutExerciseItem
{
    public int Id { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Sets { get; set; }
    public string Reps { get; set; } = string.Empty;
    public int RestSeconds { get; set; }
    public string? Notes { get; set; }
}