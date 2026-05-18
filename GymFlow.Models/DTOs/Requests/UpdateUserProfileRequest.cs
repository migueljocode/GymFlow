namespace GymFlow.Models.DTOs.Requests;

public class UpdateUserProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Goal Goal { get; set; }
    public float? Weight { get; set; }
    public float? Height { get; set; }
    public BodyType? BodyType { get; set; }
    public int? EstimatedCaloriesIntake { get; set; }
    public bool IsCompetitive { get; set; }
    public int? CoachId { get; set; }
}