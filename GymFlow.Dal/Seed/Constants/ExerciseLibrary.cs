namespace GymFlow.Dal.Seed.Constants;

/// <summary>
/// Comprehensive exercise library with real exercise data.
/// </summary>
public static class ExerciseLibrary
{
    public static List<ExerciseTemplate> GetAllExercises() => new()
    {
        // ========== CHEST EXERCISES ==========
        new() { Name = "Barbell Bench Press", MuscleGroup = MuscleGroup.Chest, Difficulty = 3, Description = "Flat bench barbell press", Equipment = "Barbell, bench" },
        new() { Name = "Incline Dumbbell Press", MuscleGroup = MuscleGroup.Chest, Difficulty = 3, Description = "45-degree incline bench", Equipment = "Dumbbells, incline bench" },
        new() { Name = "Decline Barbell Press", MuscleGroup = MuscleGroup.Chest, Difficulty = 3, Description = "Lower chest focus", Equipment = "Barbell, decline bench" },
        new() { Name = "Chest Fly (Cable)", MuscleGroup = MuscleGroup.Chest, Difficulty = 2, Description = "Cable crossover fly", Equipment = "Cable machine" },
        new() { Name = "Dumbbell Fly", MuscleGroup = MuscleGroup.Chest, Difficulty = 2, Description = "Flat bench dumbbell fly", Equipment = "Dumbbells, bench" },
        new() { Name = "Push-ups", MuscleGroup = MuscleGroup.Chest, Difficulty = 1, Description = "Bodyweight chest exercise", Equipment = "None" },
        new() { Name = "Weighted Dip", MuscleGroup = MuscleGroup.Chest, Difficulty = 4, Description = "Chest-focused dip", Equipment = "Dip bars, weight belt" },
        new() { Name = "Leverage Chest Press", MuscleGroup = MuscleGroup.Chest, Difficulty = 2, Description = "Machine chest press", Equipment = "Machine" },
        
        // ========== BACK EXERCISES ==========
        new() { Name = "Pull-ups", MuscleGroup = MuscleGroup.Back, Difficulty = 4, Description = "Wide grip pull-ups", Equipment = "Pull-up bar" },
        new() { Name = "Lat Pulldown", MuscleGroup = MuscleGroup.Back, Difficulty = 2, Description = "Cable lat pulldown", Equipment = "Cable machine" },
        new() { Name = "Barbell Row", MuscleGroup = MuscleGroup.Back, Difficulty = 3, Description = "Bent over barbell row", Equipment = "Barbell" },
        new() { Name = "Seated Cable Row", MuscleGroup = MuscleGroup.Back, Difficulty = 2, Description = "V-grip attachment", Equipment = "Cable machine" },
        new() { Name = "T-Bar Row", MuscleGroup = MuscleGroup.Back, Difficulty = 3, Description = "T-bar machine row", Equipment = "T-bar machine" },
        new() { Name = "Single Arm Dumbbell Row", MuscleGroup = MuscleGroup.Back, Difficulty = 2, Description = "Unilateral row", Equipment = "Dumbbell, bench" },
        new() { Name = "Deadlift", MuscleGroup = MuscleGroup.Back, Difficulty = 5, Description = "Conventional deadlift", Equipment = "Barbell, plates" },
        new() { Name = "Rack Pull", MuscleGroup = MuscleGroup.Back, Difficulty = 4, Description = "Partial deadlift", Equipment = "Barbell, power rack" },
        new() { Name = "Face Pull", MuscleGroup = MuscleGroup.Back, Difficulty = 2, Description = "Rear delt focus", Equipment = "Cable machine" },
        
        // ========== LEG EXERCISES ==========
        new() { Name = "Barbell Squat", MuscleGroup = MuscleGroup.Legs, Difficulty = 4, Description = "Back squat", Equipment = "Barbell, squat rack" },
        new() { Name = "Front Squat", MuscleGroup = MuscleGroup.Legs, Difficulty = 4, Description = "Barbell front squat", Equipment = "Barbell, squat rack" },
        new() { Name = "Leg Press", MuscleGroup = MuscleGroup.Legs, Difficulty = 2, Description = "Machine leg press", Equipment = "Leg press machine" },
        new() { Name = "Romanian Deadlift", MuscleGroup = MuscleGroup.Legs, Difficulty = 3, Description = "Hamstring focus", Equipment = "Barbell" },
        new() { Name = "Lunges", MuscleGroup = MuscleGroup.Legs, Difficulty = 3, Description = "Walking lunges", Equipment = "Dumbbells or barbell" },
        new() { Name = "Bulgarian Split Squat", MuscleGroup = MuscleGroup.Legs, Difficulty = 4, Description = "Rear foot elevated squat", Equipment = "Dumbbells, bench" },
        new() { Name = "Leg Extension", MuscleGroup = MuscleGroup.Legs, Difficulty = 1, Description = "Quad isolation", Equipment = "Leg extension machine" },
        new() { Name = "Leg Curl", MuscleGroup = MuscleGroup.Legs, Difficulty = 1, Description = "Hamstring isolation", Equipment = "Leg curl machine" },
        new() { Name = "Hip Thrust", MuscleGroup = MuscleGroup.Legs, Difficulty = 2, Description = "Glute activation", Equipment = "Barbell, bench" },
        new() { Name = "Calf Raise", MuscleGroup = MuscleGroup.Legs, Difficulty = 1, Description = "Standing or seated", Equipment = "Calf raise machine" },
        
        // ========== SHOULDER EXERCISES ==========
        new() { Name = "Overhead Press", MuscleGroup = MuscleGroup.Shoulders, Difficulty = 3, Description = "Barbell military press", Equipment = "Barbell" },
        new() { Name = "Seated Dumbbell Press", MuscleGroup = MuscleGroup.Shoulders, Difficulty = 3, Description = "Seated overhead press", Equipment = "Dumbbells, bench" },
        new() { Name = "Arnold Press", MuscleGroup = MuscleGroup.Shoulders, Difficulty = 3, Description = "Rotating dumbbell press", Equipment = "Dumbbells, bench" },
        new() { Name = "Lateral Raise", MuscleGroup = MuscleGroup.Shoulders, Difficulty = 2, Description = "Dumbbell lateral raise", Equipment = "Dumbbells" },
        new() { Name = "Front Raise", MuscleGroup = MuscleGroup.Shoulders, Difficulty = 2, Description = "Plate or dumbbell raise", Equipment = "Dumbbell or plate" },
        new() { Name = "Rear Delt Fly", MuscleGroup = MuscleGroup.Shoulders, Difficulty = 2, Description = "Bent over reverse fly", Equipment = "Dumbbells" },
        new() { Name = "Upright Row", MuscleGroup = MuscleGroup.Shoulders, Difficulty = 2, Description = "Barbell upright row", Equipment = "Barbell" },
        new() { Name = "Shrug", MuscleGroup = MuscleGroup.Shoulders, Difficulty = 1, Description = "Trap isolation", Equipment = "Dumbbells or barbell" },
        
        // ========== ARM EXERCISES ==========
        new() { Name = "Barbell Curl", MuscleGroup = MuscleGroup.Arms, Difficulty = 2, Description = "Standard bicep curl", Equipment = "Barbell" },
        new() { Name = "Dumbbell Curl", MuscleGroup = MuscleGroup.Arms, Difficulty = 2, Description = "Alternating or simultaneous", Equipment = "Dumbbells" },
        new() { Name = "Hammer Curl", MuscleGroup = MuscleGroup.Arms, Difficulty = 2, Description = "Neutral grip curl", Equipment = "Dumbbells" },
        new() { Name = "Preacher Curl", MuscleGroup = MuscleGroup.Arms, Difficulty = 2, Description = "Supported bicep curl", Equipment = "Preacher bench, barbell" },
        new() { Name = "Concentration Curl", MuscleGroup = MuscleGroup.Arms, Difficulty = 2, Description = "Single arm isolation", Equipment = "Dumbbell, bench" },
        new() { Name = "Tricep Pushdown", MuscleGroup = MuscleGroup.Arms, Difficulty = 2, Description = "Cable pushdown", Equipment = "Cable machine" },
        new() { Name = "Skull Crusher", MuscleGroup = MuscleGroup.Arms, Difficulty = 3, Description = "Lying tricep extension", Equipment = "EZ bar, bench" },
        new() { Name = "Overhead Tricep Extension", MuscleGroup = MuscleGroup.Arms, Difficulty = 2, Description = "Dumbbell overhead extension", Equipment = "Dumbbell" },
        new() { Name = "Dips", MuscleGroup = MuscleGroup.Arms, Difficulty = 3, Description = "Tricep focus", Equipment = "Dip bars" },
        
        // ========== CORE EXERCISES ==========
        new() { Name = "Plank", MuscleGroup = MuscleGroup.Core, Difficulty = 1, Description = "Front plank hold", Equipment = "None" },
        new() { Name = "Russian Twist", MuscleGroup = MuscleGroup.Core, Difficulty = 2, Description = "Weighted twist", Equipment = "Dumbbell or plate" },
        new() { Name = "Leg Raise", MuscleGroup = MuscleGroup.Core, Difficulty = 2, Description = "Hanging or lying", Equipment = "Pull-up bar or bench" },
        new() { Name = "Crunches", MuscleGroup = MuscleGroup.Core, Difficulty = 1, Description = "Standard crunch", Equipment = "None" },
        new() { Name = "Cable Crunch", MuscleGroup = MuscleGroup.Core, Difficulty = 2, Description = "Rope crunch", Equipment = "Cable machine" },
        new() { Name = "Ab Wheel Rollout", MuscleGroup = MuscleGroup.Core, Difficulty = 4, Description = "Advanced core exercise", Equipment = "Ab wheel" },
        new() { Name = "V-up", MuscleGroup = MuscleGroup.Core, Difficulty = 3, Description = "Bodyweight V-up", Equipment = "None" },
        new() { Name = "Side Plank", MuscleGroup = MuscleGroup.Core, Difficulty = 2, Description = "Oblique focus", Equipment = "None" },
    };
    
    /// <summary>
    /// Gets exercises filtered by muscle group.
    /// </summary>
    public static List<ExerciseTemplate> GetByMuscleGroup(MuscleGroup muscleGroup) =>
        GetAllExercises().Where(e => e.MuscleGroup == muscleGroup).ToList();
    
    /// <summary>
    /// Gets random exercises for a muscle group.
    /// </summary>
    public static List<ExerciseTemplate> GetRandomByMuscleGroup(MuscleGroup muscleGroup, int count, Random random) =>
        GetByMuscleGroup(muscleGroup).OrderBy(_ => random.Next()).Take(count).ToList();
}

/// <summary>
/// Template for exercise data.
/// </summary>
public class ExerciseTemplate
{
    public string Name { get; set; } = string.Empty;
    public MuscleGroup MuscleGroup { get; set; }
    public int Difficulty { get; set; } // 1-5
    public string Description { get; set; } = string.Empty;
    public string Equipment { get; set; } = string.Empty;
}