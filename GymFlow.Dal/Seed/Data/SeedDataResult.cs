using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Dal.Seed.Data;

/// <summary>
/// Container for generated seed data
/// </summary>
public class SeedDataResult
{
    public List<Exercise> Exercises { get; set; } = new();
    public List<Person> Persons { get; set; } = new();
    public List<User> Users { get; set; } = new();
    public List<Coach> Coaches { get; set; } = new();
    public List<WorkoutPlan> WorkoutPlans { get; set; } = new();
    public List<WorkoutDay> WorkoutDays { get; set; } = new();
    public List<WorkoutDayExercise> WorkoutDayExercises { get; set; } = new();
    public List<ProgressLog> ProgressLogs { get; set; } = new();
    public List<WorkoutSession> WorkoutSessions { get; set; } = new();
}