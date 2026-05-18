namespace GymFlow.Models.DTOs.Responses;

public class WorkoutPlanListResponse
{
    public int Id { get; set; }
    public int Phase { get; set; }
    public int SessionsPerWeek { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; }
}