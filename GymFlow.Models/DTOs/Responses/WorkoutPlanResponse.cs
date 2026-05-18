namespace GymFlow.Models.DTOs.Responses;

public class WorkoutPlanResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Phase { get; set; }
    public int SessionsPerWeek { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<WorkoutDayResponse>? WorkoutDays { get; set; }
    public int CompletedSessionsCount { get; set; }
    public int TotalSessionsCount { get; set; }
    public float? AverageWeightDuringPlan { get; set; }
}

public class WorkoutDayResponse
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public string DayName => DayOfWeek.ToString();
    public MuscleGroup TargetMuscles { get; set; }
    public string TargetMusclesDisplay => TargetMuscles.ToString();
    public int DurationMinutes { get; set; }
    public Intensity Intensity { get; set; }
    public string? Notes { get; set; }
    public List<WorkoutDayExerciseResponse>? Exercises { get; set; }
    public int TimesCompleted { get; set; }
}

public class WorkoutDayExerciseResponse
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