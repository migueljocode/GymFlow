namespace GymFlow.Models.DTOs.Responses;

public class DashboardStatsResponse
{
    // User Stats
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    
    // Workout Stats
    public int ActiveWorkoutPlans { get; set; }
    public int TotalWorkoutsThisWeek { get; set; }
    public int TotalWorkoutsThisMonth { get; set; }
    
    // Exercise Stats
    public List<PopularExerciseResponse> MostUsedExercises { get; set; } = new();
    
    // Demographics
    public Dictionary<string, int> UsersByGoal { get; set; } = new();
    public Dictionary<string, int> UsersByGender { get; set; } = new();
    public float AverageAge { get; set; }
    public float AverageWeight { get; set; }
    
    // Progress Stats
    public float AverageWeightLoss { get; set; }
    public float AverageConsistencyRate { get; set; }
    
    public DateTime GeneratedAt { get; set; }
}

public class PopularExerciseResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public int UsageCount { get; set; }
}