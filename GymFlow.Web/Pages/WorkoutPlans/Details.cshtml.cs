using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;

namespace GymFlow.Web.Pages.WorkoutPlans;

public class DetailsModel : PageModel
{
    private readonly ApiClient _apiClient;
    
    public DetailsModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    public WorkoutPlanDetailsDto? Plan { get; set; }
    public int CompletedSessions { get; set; }
    public int TotalSessions { get; set; }
    public int CompletionPercentage { get; set; }
    
    public async Task OnGetAsync(int id)
    {
        Plan = await _apiClient.GetAsync<WorkoutPlanDetailsDto>($"api/workoutplans/{id}/details");
        
        if (Plan != null && Plan.WorkoutDays != null)
        {
            TotalSessions = Plan.WorkoutDays.Count * 4;
            CompletedSessions = 0;
            CompletionPercentage = TotalSessions > 0 ? (CompletedSessions * 100 / TotalSessions) : 0;
        }
    }
    
    public async Task<IActionResult> OnPostDownloadPdfAsync(int planId)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/workout-plan/{planId}");
        if (pdfBytes == null)
            return NotFound();
        
        return File(pdfBytes, "application/pdf", $"WorkoutPlan_{planId}.pdf");
    }
}

// DTOها (همان保持不变)
public class WorkoutPlanDetailsDto
{
    public int Id { get; set; }
    public int Phase { get; set; }
    public int SessionsPerWeek { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public List<WorkoutDayDetailsDto> WorkoutDays { get; set; } = new();
}

public class WorkoutDayDetailsDto
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public int TargetMuscles { get; set; }
    public int DurationMinutes { get; set; }
    public int Intensity { get; set; }
    public string? Notes { get; set; }
    public List<ExerciseDetailDto> Exercises { get; set; } = new();
}

public class ExerciseDetailDto
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public int Sets { get; set; }
    public string Reps { get; set; } = string.Empty;
    public int RestSeconds { get; set; }
    public string? Notes { get; set; }
}