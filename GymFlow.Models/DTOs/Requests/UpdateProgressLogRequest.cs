namespace GymFlow.Models.DTOs.Requests;

public class UpdateProgressLogRequest
{
    [Range(20, 300)]
    public float? Weight { get; set; }
    
    [Range(3, 50)]
    public float? BodyFatPercentage { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}