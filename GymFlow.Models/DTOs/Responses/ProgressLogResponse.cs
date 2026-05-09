namespace GymFlow.Models.DTOs.Responses;

public class ProgressLogResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? WorkoutPlanId { get; set; }
    public DateOnly LogDate { get; set; }
    public float Weight { get; set; }
    public float? BodyFatPercentage { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}