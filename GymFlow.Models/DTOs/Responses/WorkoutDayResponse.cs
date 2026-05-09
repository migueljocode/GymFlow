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