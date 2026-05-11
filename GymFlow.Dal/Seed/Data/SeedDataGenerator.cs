using GymFlow.Dal.Seed.Generators;

namespace GymFlow.Dal.Seed.Data;

/// <summary>
/// Orchestrates the generation of all seed data using specialized generators
/// </summary>
public class SeedDataGenerator
{
    private readonly SeedOptions _options;
    
    public SeedDataGenerator(SeedOptions options)
    {
        _options = options;
    }
    
    public SeedDataResult GenerateAllData()
    {
        Console.WriteLine("🎲 Generating rich test data with Bogus...");
        
        var result = new SeedDataResult();
        
        // 1. Generate exercises
        var workoutGen = new WorkoutGenerator(_options);
        result.Exercises = workoutGen.GenerateExercises();
        Console.WriteLine($"  ✓ Generated {result.Exercises.Count} exercises");
        
        // 2. Generate Persons and their roles
        GeneratePersonsAndRoles(result);
        Console.WriteLine($"  ✓ Generated {result.Persons.Count} persons");
        Console.WriteLine($"     - {result.Users.Count} users");
        Console.WriteLine($"     - {result.Coaches.Count} coaches");
        
        // 3. Generate workout data for Users
        GenerateWorkoutData(result, workoutGen);
        
        Console.WriteLine($"  ✓ Generated {result.WorkoutPlans.Count} workout plans");
        Console.WriteLine($"  ✓ Generated {result.WorkoutDays.Count} workout days");
        Console.WriteLine($"  ✓ Generated {result.WorkoutDayExercises.Count} workout day exercises");
        Console.WriteLine($"  ✓ Generated {result.ProgressLogs.Count} progress logs");
        Console.WriteLine($"  ✓ Generated {result.WorkoutSessions.Count} workout sessions");
        
        return result;
    }
    
    private void GeneratePersonsAndRoles(SeedDataResult result)
    {
        var personGen = new PersonGenerator(_options);
        var userGen = new UserGenerator(_options);
        var coachGen = new CoachGenerator(_options);
        
        // Demo Coach
        var coachPerson = personGen.CreateDemoCoach();
        result.Persons.Add(coachPerson);
        var coach = coachGen.CreateDemoFromPerson(coachPerson);
        result.Coaches.Add(coach);
        coachPerson.Coach = coach;
        Console.WriteLine($"   ✅ Demo Coach created: username='coach', password='coach123'");
        
        // Demo Member
        var memberPerson = personGen.CreateDemoMember();
        result.Persons.Add(memberPerson);
        var memberUser = userGen.CreateFromPerson(memberPerson, isDemo: true);
        result.Users.Add(memberUser);
        memberPerson.User = memberUser;
        Console.WriteLine($"   ✅ Demo User created: username='member', password='member123'");
        
        // Random Users
        for (int i = 0; i < _options.UserCount; i++)
        {
            var person = personGen.CreateRandom();
            result.Persons.Add(person);
            var user = userGen.CreateFromPerson(person);
            result.Users.Add(user);
            person.User = user;
        }
        
        // Random Coaches (optional)
        var coachCount = Math.Max(1, _options.UserCount / 5);
        for (int i = 0; i < coachCount; i++)
        {
            var person = personGen.CreateRandom();
            result.Persons.Add(person);
            var newCoach = coachGen.CreateRandomFromPerson(person);
            result.Coaches.Add(newCoach);
            person.Coach = newCoach;
        }
    }
    
    private void GenerateWorkoutData(SeedDataResult result, WorkoutGenerator workoutGen)
    {
        var random = new Random();
        var progressGen = new ProgressGenerator(_options);
        var allWorkoutPlans = new List<WorkoutPlan>();
        var allWorkoutDays = new List<WorkoutDay>();
        var allWorkoutDayExercises = new List<WorkoutDayExercise>();
        var allProgressLogs = new List<ProgressLog>();
        var allWorkoutSessions = new List<WorkoutSession>();
        
        foreach (var user in result.Users)
        {
            var planCount = random.Next(_options.MinWorkoutPlansPerUser, _options.MaxWorkoutPlansPerUser + 1);
            var plans = workoutGen.GenerateWorkoutPlans(user, planCount);
            allWorkoutPlans.AddRange(plans);
            
            foreach (var plan in plans)
            {
                var dayCount = random.Next(_options.MinWorkoutDaysPerPlan, _options.MaxWorkoutDaysPerPlan + 1);
                var days = workoutGen.GenerateWorkoutDays(plan, dayCount);
                allWorkoutDays.AddRange(days);
                
                foreach (var day in days)
                {
                    var exerciseCount = random.Next(_options.MinExercisesPerDay, _options.MaxExercisesPerDay + 1);
                    var dayExercises = GenerateWorkoutDayExercises(day, result.Exercises, exerciseCount);
                    allWorkoutDayExercises.AddRange(dayExercises);
                    
                    var sessions = progressGen.GenerateWorkoutSessions(day, plan);
                    allWorkoutSessions.AddRange(sessions);
                }
                
                var person = user.Person;
                var logs = progressGen.GenerateProgressLogs(user, plan, person);
                allProgressLogs.AddRange(logs);
            }
        }
        
        result.WorkoutPlans = allWorkoutPlans;
        result.WorkoutDays = allWorkoutDays;
        result.WorkoutDayExercises = allWorkoutDayExercises;
        result.ProgressLogs = allProgressLogs;
        result.WorkoutSessions = allWorkoutSessions;
    }
    
    private List<WorkoutDayExercise> GenerateWorkoutDayExercises(WorkoutDay day, List<Exercise> exercises, int exerciseCount)
    {
        var dayExercises = new List<WorkoutDayExercise>();
        var random = new Random();
        var faker = new Faker();
        
        var relevantExercises = exercises
            .Where(e => day.TargetMuscles.HasFlag(e.PrimaryMuscleGroup) || 
                       (day.TargetMuscles == MuscleGroup.None))
            .ToList();
        
        if (!relevantExercises.Any())
            relevantExercises = exercises.Take(10).ToList();
        
        var selectedExercises = relevantExercises
            .OrderBy(_ => random.Next())
            .Take(Math.Min(exerciseCount, relevantExercises.Count))
            .ToList();
        
        var exerciseId = 1;
        foreach (var exercise in selectedExercises)
        {
            var sets = faker.Random.Int(3, 5);
            var reps = faker.Random.Int(8, 15);
            var restSeconds = sets > 4 ? faker.Random.Int(90, 120) : faker.Random.Int(45, 75);
            
            var wde = new WorkoutDayExercise
            {
                Id = exerciseId++,
                WorkoutDayId = day.Id,
                ExerciseId = exercise.Id,
                Sets = sets,
                Reps = $"{reps},{reps - (reps > 10 ? 2 : 1)},{reps - (reps > 10 ? 4 : 2)}",
                RestSeconds = restSeconds,
                Notes = faker.Random.Bool(0.15f) ? GetExerciseNote(exercise.Name) : null,
                CreatedAt = day.CreatedAt
            };
            
            dayExercises.Add(wde);
        }
        
        return dayExercises;
    }
    
    private string GetExerciseNote(string exerciseName) => exerciseName switch
    {
        "Deadlift" => "Focus on keeping back straight",
        "Squat" => "Go deeper than usual, feeling good",
        "Bench Press" => "Elbows at 45 degrees",
        "Pull-ups" => "Full range of motion, no kipping",
        _ => "Slow negative, explosive positive"
    };
}