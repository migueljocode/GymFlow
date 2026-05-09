namespace GymFlow.Dal.Seed.Data;

/// <summary>
/// Advanced data generator using Bogus for realistic test data.
/// </summary>
public class SeedDataGenerator
{
    private readonly Random _random;
    private readonly Faker _faker;
    private readonly SeedOptions _options;
    
    private int _userId = 1;
    private int _planId = 1;
    private int _workoutDayId = 1;
    private int _logId = 1;
    private int _sessionId = 1;
    
    public SeedDataGenerator(SeedOptions options)
    {
        _options = options;
        _random = options.RandomSeed.HasValue 
            ? new Random(options.RandomSeed.Value) 
            : new Random();
        
        Randomizer.Seed = _random;
        _faker = new Faker("en");
    }
    
    /// <summary>
    /// Generates the complete seed data set with all relationships.
    /// </summary>
    public SeedDataResult GenerateAllData()
    {
        Console.WriteLine("🎲 Generating rich test data with Bogus...");
        
        var result = new SeedDataResult();
        
        // 1. Generate exercises library (always the same)
        result.Exercises = GenerateExercises();
        Console.WriteLine($"  ✓ Generated {result.Exercises.Count} exercises");
        
        // 2. Generate users
        result.Users = GenerateUsers();
        Console.WriteLine($"  ✓ Generated {result.Users.Count} users");
        
        var allWorkoutPlans = new List<WorkoutPlan>();
        var allWorkoutDays = new List<WorkoutDay>();
        var allWorkoutDayExercises = new List<WorkoutDayExercise>();
        var allProgressLogs = new List<ProgressLog>();
        var allWorkoutSessions = new List<WorkoutSession>();
        
        // 3. For each user, generate complete history
        foreach (var user in result.Users)
        {
            // Generate workout plans
            var planCount = _faker.Random.Int(_options.MinWorkoutPlansPerUser, _options.MaxWorkoutPlansPerUser);
            var plans = GenerateWorkoutPlans(user, planCount);
            allWorkoutPlans.AddRange(plans);
            
            foreach (var plan in plans)
            {
                // Generate workout days
                var dayCount = _faker.Random.Int(_options.MinWorkoutDaysPerPlan, _options.MaxWorkoutDaysPerPlan);
                var days = GenerateWorkoutDays(plan, dayCount);
                allWorkoutDays.AddRange(days);
                
                foreach (var day in days)
                {
                    // Generate exercises for this day
                    var exerciseCount = _faker.Random.Int(_options.MinExercisesPerDay, _options.MaxExercisesPerDay);
                    var dayExercises = GenerateWorkoutDayExercises(day, result.Exercises, exerciseCount);
                    allWorkoutDayExercises.AddRange(dayExercises);
                    
                    // Generate completed sessions
                    var sessions = GenerateWorkoutSessions(day, plan);
                    allWorkoutSessions.AddRange(sessions);
                }
                
                // Generate progress logs for this plan
                var logs = GenerateProgressLogs(user, plan);
                allProgressLogs.AddRange(logs);
            }
        }
        
        result.WorkoutPlans = allWorkoutPlans;
        result.WorkoutDays = allWorkoutDays;
        result.WorkoutDayExercises = allWorkoutDayExercises;
        result.ProgressLogs = allProgressLogs;
        result.WorkoutSessions = allWorkoutSessions;
        
        Console.WriteLine($"  ✓ Generated {allWorkoutPlans.Count} workout plans");
        Console.WriteLine($"  ✓ Generated {allWorkoutDays.Count} workout days");
        Console.WriteLine($"  ✓ Generated {allWorkoutDayExercises.Count} workout day exercises");
        Console.WriteLine($"  ✓ Generated {allProgressLogs.Count} progress logs");
        Console.WriteLine($"  ✓ Generated {allWorkoutSessions.Count} workout sessions");
        
        return result;
    }
    
    /// <summary>
    /// Generates exercise library from constants.
    /// </summary>
    private List<Exercise> GenerateExercises()
    {
        var exercises = new List<Exercise>();
        var templateId = 1;
        
        foreach (var template in ExerciseLibrary.GetAllExercises())
        {
            exercises.Add(new Exercise
            {
                Id = templateId++,
                Name = template.Name,
                PrimaryMuscleGroup = template.MuscleGroup,
                Description = $"{template.Description} - Equipment: {template.Equipment}",
                CreatedAt = DateTime.UtcNow.AddMonths(-12),
                UpdatedAt = null,
                IsDeleted = false,
                DeletedAt = null
            });
        }
        
        return exercises;
    }
    
    /// <summary>
    /// Generizes realistic users with varied profiles.
    /// </summary>
    private List<User> GenerateUsers()
    {
        var users = new List<User>();
        
        // Generate random users
        for (int i = 0; i < _options.UserCount; i++)
        {
            var gender = _faker.PickRandom<Gender>();
            var weight = gender == Gender.Female 
                ? _faker.Random.Float(50f, 85f) 
                : _faker.Random.Float(65f, 110f);
            var height = gender == Gender.Female 
                ? _faker.Random.Float(155f, 175f) 
                : _faker.Random.Float(170f, 190f);
            
            var user = new User
            {
                Id = _userId++,
                FirstName = _faker.Name.FirstName(gender == Gender.Male ? Bogus.DataSets.Name.Gender.Male : Bogus.DataSets.Name.Gender.Female),
                LastName = _faker.Name.LastName(),
                Email = _faker.Internet.Email(),
                Phone = _faker.Phone.PhoneNumber(),
                Gender = gender,
                Age = _faker.Random.Int(18, 55),
                Weight = weight,
                Height = height,
                BodyType = DetermineBodyType(weight, height),
                Goal = _faker.PickRandom<Goal>(),
                EstimatedCaloriesIntake = _faker.Random.Bool(0.7f) ? _faker.Random.Int(1800, 3200) : null,
                IsCompetitive = _faker.Random.Bool(0.15f),
                CreatedAt = _faker.Date.Past(1),
                UpdatedAt = _faker.Random.Bool(0.3f) ? _faker.Date.Recent(30) : null,
                IsDeleted = false,
                DeletedAt = null
            };
            
            users.Add(user);
        }
        
        // Add demo user if requested
        if (_options.IncludeDemoUser)
        {
            var demoUser = new User
            {
                Id = _userId++,
                FirstName = _options.DemoUserFirstName,
                LastName = _options.DemoUserLastName,
                Email = _options.DemoUserEmail,
                Phone = "+1 (555) 000-0000",
                Gender = Gender.Other,
                Age = 28,
                Weight = 78.5f,
                Height = 178f,
                BodyType = BodyType.Fit,
                Goal = Goal.MuscleGain,
                EstimatedCaloriesIntake = 2800,
                IsCompetitive = false,
                CreatedAt = DateTime.UtcNow.AddDays(-45),
                UpdatedAt = null,
                IsDeleted = false,
                DeletedAt = null
            };
            users.Add(demoUser);
        }
        
        return users;
    }
    
    /// <summary>
    /// Determines body type based on BMI and activity assumptions.
    /// </summary>
    private BodyType DetermineBodyType(float weight, float height)
    {
        var bmi = weight / ((height / 100) * (height / 100));
        
        return bmi switch
        {
            < 18.5f => BodyType.LeanMuscular,
            < 25 => BodyType.Fit,
            < 30 => BodyType.Overweight,
            _ => BodyType.Obese
        };
    }
    
    /// <summary>
    /// Generates workout plans for a user across different phases.
    /// </summary>
    private List<WorkoutPlan> GenerateWorkoutPlans(User user, int planCount)
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
                CreatedAt = currentDate,
                UpdatedAt = null,
                IsDeleted = false,
                DeletedAt = null
            };
            
            plans.Add(plan);
            currentDate = currentDate.AddMonths(2);
        }
        
        return plans;
    }
    
    /// <summary>
    /// Generates workout days for a plan with appropriate muscle group targeting.
    /// </summary>
    private List<WorkoutDay> GenerateWorkoutDays(WorkoutPlan plan, int dayCount)
    {
        var days = new List<WorkoutDay>();
        var availableDays = Enum.GetValues<DayOfWeek>()
            .Where(d => d != DayOfWeek.Saturday && d != DayOfWeek.Sunday)
            .ToList();
        
        var selectedDays = availableDays
            .OrderBy(_ => _random.Next())
            .Take(dayCount)
            .OrderBy(d => d)
            .ToList();
        
        // Common splits for realistic programming
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
                Notes = _faker.Random.Bool(0.2f) ? $"Focus on {GetMuscleGroupName(targetMuscles)}" : null,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = null,
                IsDeleted = false,
                DeletedAt = null
            };
            
            days.Add(day);
        }
        
        return days;
    }
    
    /// <summary>
    /// Gets realistic workout splits based on sessions per week.
    /// </summary>
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
                splits.Add(MuscleGroup.FullBody);
                splits.Add(MuscleGroup.FullBody);
                splits.Add(MuscleGroup.FullBody);
                break;
        }
        
        return splits;
    }
    
    /// <summary>
    /// Generates a random combination of muscle groups.
    /// </summary>
    private MuscleGroup GetRandomMuscleGroupCombination()
    {
        if (_faker.Random.Double() < _options.CompoundWorkoutProbability)
        {
            var groups = new[] { MuscleGroup.Chest, MuscleGroup.Back, MuscleGroup.Legs, 
                                  MuscleGroup.Shoulders, MuscleGroup.Arms, MuscleGroup.Core };
            var selected = groups.OrderBy(_ => _random.Next()).Take(2).ToArray();
            return selected[0] | selected[1];
        }
        
        var singleGroups = new[] { MuscleGroup.Chest, MuscleGroup.Back, MuscleGroup.Legs, 
                                   MuscleGroup.Shoulders, MuscleGroup.Arms, MuscleGroup.Core };
        return _faker.PickRandom(singleGroups);
    }
    
    /// <summary>
    /// Generates exercises for a workout day.
    /// </summary>
    private List<WorkoutDayExercise> GenerateWorkoutDayExercises(WorkoutDay day, List<Exercise> exercises, int exerciseCount)
    {
        var dayExercises = new List<WorkoutDayExercise>();
        
        // Get relevant exercises for the target muscle groups
        var relevantExercises = exercises
            .Where(e => day.TargetMuscles.HasFlag(e.PrimaryMuscleGroup) || 
                       (day.TargetMuscles == MuscleGroup.None))
            .ToList();
        
        if (!relevantExercises.Any())
            relevantExercises = exercises.Take(10).ToList();
        
        var selectedExercises = relevantExercises
            .OrderBy(_ => _random.Next())
            .Take(Math.Min(exerciseCount, relevantExercises.Count))
            .ToList();
        
        foreach (var exercise in selectedExercises)
        {
            var sets = _faker.Random.Int(3, 5);
            var reps = _faker.Random.Int(8, 15);
            var restSeconds = sets > 4 ? _faker.Random.Int(90, 120) : _faker.Random.Int(45, 75);
            
            var wde = new WorkoutDayExercise
            {
                WorkoutDayId = day.Id,
                ExerciseId = exercise.Id,
                Sets = sets,
                Reps = $"{reps},{reps - (reps > 10 ? 2 : 1)},{reps - (reps > 10 ? 4 : 2)}",
                RestSeconds = restSeconds,
                Notes = _faker.Random.Bool(0.15f) ? GetExerciseNote(exercise.Name) : null,
                CreatedAt = day.CreatedAt,
                UpdatedAt = null,
                IsDeleted = false,
                DeletedAt = null
            };
            
            dayExercises.Add(wde);
        }
        
        return dayExercises;
    }
    
    /// <summary>
    /// Generates progress logs with realistic weight trends - NO DUPLICATE DATES
    /// </summary>
    private List<ProgressLog> GenerateProgressLogs(User user, WorkoutPlan plan)
    {
        var logs = new List<ProgressLog>();
        var logCount = _faker.Random.Int(Math.Min(5, _options.MinProgressLogsPerUser / 2), 
                                        Math.Min(15, _options.MaxProgressLogsPerUser / 2));
        
        var startDate = plan.StartDate;
        var endDate = plan.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var dateRange = endDate.DayNumber - startDate.DayNumber;
        
        if (dateRange <= 0) return logs;
        
        var currentWeight = user.Weight ?? 75f;
        var usedDates = new HashSet<DateOnly>();  // ← جلوگیری از تاریخ تکراری
        
        // تعیین روند وزن بر اساس هدف کاربر
        var isWeightLossPlan = user.Goal == Goal.FatLoss;
        
        for (int i = 0; i < logCount && usedDates.Count < logCount; i++)
        {
            // پیدا کردن تاریخی که قبلاً استفاده نشده باشد
            DateOnly logDate;
            int attempts = 0;
            do
            {
                var daysOffset = _faker.Random.Int(3, Math.Max(7, dateRange / logCount));
                logDate = startDate.AddDays(daysOffset);
                attempts++;
                if (attempts > 50) break; // جلوگیری از حلقه بی‌نهایت
            } while (logDate > endDate || usedDates.Contains(logDate));
            
            if (logDate > endDate || usedDates.Contains(logDate)) continue;
            usedDates.Add(logDate);
            
            // شبیه‌سازی روند وزنی واقعی
            var weeklyChange = isWeightLossPlan 
                ? _faker.Random.Float(-0.5f, -0.2f) 
                : _faker.Random.Float(-0.1f, 0.3f);
            
            var daysSinceLastLog = i == 0 ? 7 : (logDate.DayNumber - logs.Last().LogDate.DayNumber);
            var weightChange = weeklyChange * (daysSinceLastLog / 7f);
            
            currentWeight += weightChange;
            currentWeight = Math.Clamp(currentWeight, 45f, 130f);
            
            var log = new ProgressLog
            {
                Id = _logId++,
                UserId = user.Id,
                WorkoutPlanId = plan.Id,
                LogDate = logDate,
                Weight = (float)Math.Round(currentWeight, 1),
                BodyFatPercentage = _faker.Random.Double() < _options.BodyFatPercentageInclusionRate 
                    ? (float?)Math.Round(_faker.Random.Float(8f, 30f), 1) 
                    : null,
                Notes = _faker.Random.Bool(0.2f) ? GetProgressNote(currentWeight, user.Goal) : null,
                CreatedAt = logDate.ToDateTime(TimeOnly.MinValue),
                UpdatedAt = null,
                IsDeleted = false,
                DeletedAt = null
            };
            
            logs.Add(log);
        }
        
        // مرتب‌سازی بر اساس تاریخ
        return logs.OrderBy(l => l.LogDate).ToList();
    }
    
    /// <summary>
    /// Generates completed workout sessions based on planned days.
    /// </summary>
    private List<WorkoutSession> GenerateWorkoutSessions(WorkoutDay workoutDay, WorkoutPlan plan)
    {
        var sessions = new List<WorkoutSession>();
        var startDate = plan.StartDate;
        var endDate = plan.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        
        // Calculate first occurrence of this day of week
        var currentDate = startDate;
        var daysUntilTarget = ((int)workoutDay.DayOfWeek - (int)currentDate.DayOfWeek + 7) % 7;
        currentDate = currentDate.AddDays(daysUntilTarget);
        
        var feelings = new[] { "Energetic! 💪 Did PR on bench", "Good session, felt strong", 
                               "Tired today, went lighter", "Great pump! 🔥", 
                               "Knee slightly sore, skipped squats", "Amazing energy!",
                               "Decent workout, need more sleep", "Personal best on deadlift! 🏆",
                               "Felt weak, took a deload day", "Crushed it! ⚡" };
        
        while (currentDate <= endDate)
        {
            // Determine if session was completed
            var completed = _random.NextDouble() < _options.WorkoutSessionCompletionRate;
            
            if (completed)
            {
                var session = new WorkoutSession
                {
                    Id = _sessionId++,
                    WorkoutDayId = workoutDay.Id,
                    ActualDate = currentDate,
                    ActualDurationMinutes = workoutDay.DurationMinutes + _faker.Random.Int(-10, 15),
                    Feeling = _faker.Random.Double() < _options.FeelingNoteProbability 
                        ? _faker.PickRandom(feelings) 
                        : null,
                    CreatedAt = currentDate.ToDateTime(TimeOnly.MinValue).AddHours(_faker.Random.Int(6, 20)),
                    UpdatedAt = null,
                    IsDeleted = false,
                    DeletedAt = null
                };
                sessions.Add(session);
            }
            
            currentDate = currentDate.AddDays(7);
        }
        
        return sessions;
    }
    
    // Helper methods for generating realistic text
    private string GetMuscleGroupName(MuscleGroup muscles) => muscles switch
    {
        MuscleGroup.Chest => "Chest",
        MuscleGroup.Back => "Back",
        MuscleGroup.Legs => "Legs",
        MuscleGroup.Shoulders => "Shoulders",
        MuscleGroup.Arms => "Arms",
        MuscleGroup.Core => "Core",
        _ when muscles.HasFlag(MuscleGroup.Chest) && muscles.HasFlag(MuscleGroup.Arms) => "Chest & Arms",
        _ when muscles.HasFlag(MuscleGroup.Back) && muscles.HasFlag(MuscleGroup.Shoulders) => "Back & Shoulders",
        _ when muscles.HasFlag(MuscleGroup.Legs) && muscles.HasFlag(MuscleGroup.Core) => "Legs & Core",
        _ => "Full Body"
    };
    
    private string GenerateWorkoutPlanNotes(int phase) => phase switch
    {
        1 => "Foundation phase - focus on form and building work capacity",
        2 => "Hypertrophy phase - increasing volume and intensity",
        3 => "Strength phase - heavier weights, lower reps",
        4 => "Peaking phase - intensity focus",
        _ => "Maintenance phase - balance of strength and conditioning"
    };
    
    private string GetExerciseNote(string exerciseName) => exerciseName switch
    {
        "Deadlift" => "Focus on keeping back straight",
        "Squat" => "Go deeper than usual, feeling good",
        "Bench Press" => "Elbows at 45 degrees",
        "Pull-ups" => "Full range of motion, no kipping",
        _ => "Slow negative, explosive positive"
    };
    
    private string GetProgressNote(float weight, Goal goal) => goal switch
    {
        Goal.FatLoss when weight < 70 => "Finally seeing abs definition!",
        Goal.MuscleGain when weight > 80 => "Arms feel fuller, vein visibility increasing",
        Goal.Fitness => "Energy levels are great, recovery improving",
        _ => "Consistent progress week over week"
    };
}

/// <summary>
/// Container for generated seed data.
/// </summary>
public class SeedDataResult
{
    public List<Exercise> Exercises { get; set; } = new();
    public List<User> Users { get; set; } = new();
    public List<WorkoutPlan> WorkoutPlans { get; set; } = new();
    public List<WorkoutDay> WorkoutDays { get; set; } = new();
    public List<WorkoutDayExercise> WorkoutDayExercises { get; set; } = new();
    public List<ProgressLog> ProgressLogs { get; set; } = new();
    public List<WorkoutSession> WorkoutSessions { get; set; } = new();
}