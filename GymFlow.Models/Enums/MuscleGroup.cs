namespace GymFlow.Models.Enums;

/// <summary>
/// Target muscle groups using Flags attribute to allow combinations (e.g., Chest | Back).
/// </summary>
[Flags]
public enum MuscleGroup
{
    None = 0,
    Chest = 1,
    Back = 2,
    Legs = 4,
    Shoulders = 8,
    Arms = 16,
    Core = 32
}
