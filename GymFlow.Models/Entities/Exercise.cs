namespace GymFlow.Models.Entities;

/// <summary>
/// A library of exercise movements (e.g., "Barbell Bench Press").
/// Used in WorkoutDayExercise to define which exercises are planned.
/// </summary>
public class Exercise : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public MuscleGroup PrimaryMuscleGroup { get; set; } // flags enum
    public string? Description { get; set; }
}