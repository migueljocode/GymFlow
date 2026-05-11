using Bogus;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;
using GymFlow.Dal.Seed.Data;
using GymFlow.Dal.Seed.Constants;

namespace GymFlow.Dal.Seed.Generators;

/// <summary>
/// Generator for creating workout-related entities (Exercises, WorkoutPlans, WorkoutDays)
/// </summary>
public class WorkoutGenerator
{
    private readonly Faker _faker;
    private readonly SeedOptions _options;
    private int _planId;
    private int _workoutDayId;
    
    public WorkoutGenerator(SeedOptions options, int startPlanId = 1, int startWorkoutDayId = 1)
    {
        _options = options;
        _planId = startPlanId;
        _workoutDayId = startWorkoutDayId;
        _faker = new Faker("en");
        Randomizer.Seed = new Random(_options.RandomSeed ?? 42);
    }
    
    public List<Exercise> GenerateExercises()
    {
        var exercises = new List<Exercise>();
        var exerciseId = 1;
        
        foreach (var template in ExerciseLibrary.GetAllExercises())
        {
            exercises.Add(new Exercise
            {
                Id = exerciseId++,
                Name = template.Name,
                PrimaryMuscleGroup = template.MuscleGroup,
                Description = $"{template.Description} - Equipment: {template.Equipment}",
                CreatedAt = DateTime.UtcNow.AddMonths(-12)
            });
        }
        
        return exercises;
    }
    
    public List<WorkoutPlan> GenerateWorkoutPlans(User user, int planCount)
    {
        var plans = new List<WorkoutPlan>();
        var currentDate = DateTime.UtcNow.AddMonths(-(planCount * 2 + 1));
        
        for (int i = 0; i < planCount; i++)
        {
            var phase = i + 1;
            var startDate = DateOnly.FromDateTime(currentDate);
            var endDate = startDate.AddMonths(2);
            var isActive = i == planCount - 1;
            
            var plan = new WorkoutPlan
            {
                Id = _planId++,
                UserId = user.Id,
                Phase = phase,
                SessionsPerWeek = _faker.Random.Int(3, 5),
                StartDate = startDate,
                EndDate = isActive ? null : endDate,
                IsActive = isActive,
                Notes = _faker.Random.Bool(0.4f) ? GenerateWorkoutPlanNotes(phase) : null,
                CreatedAt = currentDate
            };
            
            plans.Add(plan);
            currentDate = currentDate.AddMonths(2);
        }
        
        return plans;
    }
    
    public List<WorkoutDay> GenerateWorkoutDays(WorkoutPlan plan, int dayCount)
    {
        var days = new List<WorkoutDay>();
        var availableDays = Enum.GetValues<DayOfWeek>()
            .Where(d => d != DayOfWeek.Saturday && d != DayOfWeek.Sunday)
            .ToList();
        
        var selectedDays = availableDays
            .OrderBy(_ => Random.Shared.Next())
            .Take(dayCount)
            .OrderBy(d => d)
            .ToList();
        
        var splits = GetWorkoutSplits(plan.SessionsPerWeek);
        
        for (int i = 0; i < selectedDays.Count; i++)
        {
            var targetMuscles = i < splits.Count 
                ? splits[i] 
                : GetRandomMuscleGroupCombination();
            
            var day = new WorkoutDay
            {
                Id = _workoutDayId++,
                WorkoutPlanId = plan.Id,
                DayOfWeek = selectedDays[i],
                TargetMuscles = targetMuscles,
                DurationMinutes = _faker.Random.Int(45, 90),
                Intensity = _faker.PickRandom<Intensity>(),
                Notes = _faker.Random.Bool(0.2f) ? $"Focus on {targetMuscles}" : null,
                CreatedAt = plan.CreatedAt
            };
            
            days.Add(day);
        }
        
        return days;
    }
    
    private List<MuscleGroup> GetWorkoutSplits(int sessionsPerWeek)
    {
        var splits = new List<MuscleGroup>();
        
        switch (sessionsPerWeek)
        {
            case 3:
                splits.Add(MuscleGroup.Chest | MuscleGroup.Arms);
                splits.Add(MuscleGroup.Back | MuscleGroup.Shoulders);
                splits.Add(MuscleGroup.Legs | MuscleGroup.Core);
                break;
            case 4:
                splits.Add(MuscleGroup.Chest | MuscleGroup.Arms);
                splits.Add(MuscleGroup.Back | MuscleGroup.Shoulders);
                splits.Add(MuscleGroup.Legs);
                splits.Add(MuscleGroup.Core | MuscleGroup.Arms);
                break;
            case 5:
                splits.Add(MuscleGroup.Chest);
                splits.Add(MuscleGroup.Back);
                splits.Add(MuscleGroup.Legs);
                splits.Add(MuscleGroup.Shoulders | MuscleGroup.Arms);
                splits.Add(MuscleGroup.Core);
                break;
            default:
                splits.Add(MuscleGroup.None);
                splits.Add(MuscleGroup.None);
                splits.Add(MuscleGroup.None);
                break;
        }
        
        return splits;
    }
    
    private MuscleGroup GetRandomMuscleGroupCombination()
    {
        if (_faker.Random.Double() < _options.CompoundWorkoutProbability)
        {
            var groups = new[] { MuscleGroup.Chest, MuscleGroup.Back, MuscleGroup.Legs, 
                                  MuscleGroup.Shoulders, MuscleGroup.Arms, MuscleGroup.Core };
            var selected = groups.OrderBy(_ => Random.Shared.Next()).Take(2).ToArray();
            return selected[0] | selected[1];
        }
        
        var singleGroups = new[] { MuscleGroup.Chest, MuscleGroup.Back, MuscleGroup.Legs, 
                                   MuscleGroup.Shoulders, MuscleGroup.Arms, MuscleGroup.Core };
        return _faker.PickRandom(singleGroups);
    }
    
    private string GenerateWorkoutPlanNotes(int phase) => phase switch
    {
        1 => "Foundation phase - focus on form and building work capacity",
        2 => "Hypertrophy phase - increasing volume and intensity",
        3 => "Strength phase - heavier weights, lower reps",
        4 => "Peaking phase - intensity focus",
        _ => "Maintenance phase - balance of strength and conditioning"
    };
    
    public int CurrentPlanId => _planId;
    public int CurrentWorkoutDayId => _workoutDayId;
}