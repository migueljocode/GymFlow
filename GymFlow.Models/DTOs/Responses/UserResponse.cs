namespace GymFlow.Models.DTOs.Responses;

public class UserResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public float? Weight { get; set; }
    public float? Height { get; set; }
    public BodyType? BodyType { get; set; }
    public Goal Goal { get; set; }
    public int? EstimatedCaloriesIntake { get; set; }
    public bool IsCompetitive { get; set; }
    public int WorkoutPlansCount { get; set; }
    public int ProgressLogsCount { get; set; }
    public int TotalWorkoutSessions { get; set; }
    public DateTime CreatedAt { get; set; }
}