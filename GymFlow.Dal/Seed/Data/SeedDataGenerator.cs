using GymFlow.Models.Entities;
using GymFlow.Models.Enums;
using Bogus;
using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Dal.Seed.Data;

public class SeedDataGenerator
{
    private readonly SeedOptions _options;
    private readonly Faker _faker;
    private readonly Random _random;
    
    public SeedDataGenerator(SeedOptions options)
    {
        _options = options;
        _faker = new Faker("en");
        _random = new Random(options.RandomSeed ?? 42);
        if (options.RandomSeed.HasValue)
            Randomizer.Seed = new Random(options.RandomSeed.Value);
    }
    
    public List<Exercise> GenerateExercises()
    {
        var exercises = new List<Exercise>();
        
        var exerciseNames = new[]
        {
            "Bench Press", "Incline Press", "Decline Press", "Push-ups", "Chest Fly",
            "Pull-ups", "Lat Pulldown", "Barbell Row", "Seated Row", "Deadlift",
            "Squat", "Leg Press", "Leg Extension", "Leg Curl", "Lunges",
            "Overhead Press", "Lateral Raise", "Front Raise", "Face Pull", "Shrugs",
            "Bicep Curl", "Tricep Pushdown", "Hammer Curl", "Skull Crusher", "Dips",
            "Plank", "Russian Twist", "Leg Raise", "Crunches", "Ab Wheel"
        };
        
        foreach (var name in exerciseNames)
        {
            exercises.Add(new Exercise
            {
                Name = name,
                PrimaryMuscleGroup = _faker.PickRandom<MuscleGroup>(),
                Description = $"{name} - standard form",
                CreatedAt = DateTime.UtcNow.AddMonths(-12)
            });
        }
        
        return exercises;
    }
    
    public List<Person> GeneratePersons()
    {
        var persons = new List<Person>();
        var nextId = 1;
        
        // Demo Coach Person
        persons.Add(new Person
        {
            Id = nextId++,
            FirstName = "Master",
            LastName = "Coach",
            Username = "coach",
            Password = "coach123",
            Email = "coach@gymflow.com",
            Gender = Gender.Male,
            Age = 35,
            Weight = 85.5f,
            Height = 182f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        });
        
        // Demo Member Person
        persons.Add(new Person
        {
            Id = nextId++,
            FirstName = "John",
            LastName = "Doe",
            Username = "member",
            Password = "member123",
            Email = "member@gymflow.com",
            Gender = Gender.Male,
            Age = 25,
            Weight = 75.0f,
            Height = 175f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        });
        
        // Random Users Persons
        for (int i = 0; i < _options.UserCount; i++)
        {
            var gender = _faker.PickRandom<Gender>();
            var weight = gender == Gender.Female ? _faker.Random.Float(50f, 85f) : _faker.Random.Float(70f, 110f);
            var height = gender == Gender.Female ? _faker.Random.Float(155f, 175f) : _faker.Random.Float(170f, 190f);
            
            persons.Add(new Person
            {
                Id = nextId++,
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName(),
                Username = _faker.Internet.UserName(),
                Password = "password123",
                Email = _faker.Internet.Email(),
                Gender = gender,
                Age = _faker.Random.Int(18, 55),
                Weight = weight,
                Height = height,
                BodyType = DetermineBodyType(weight, height),
                CreatedAt = _faker.Date.Past(1)
            });
        }
        
        return persons;
    }
    
    public List<User> GenerateUsers(List<Person> persons)
    {
        var users = new List<User>();
        var nextId = 1;
        var personDict = persons.ToDictionary(p => p.Username, p => p);
        
        // Demo User
        if (personDict.TryGetValue("member", out var memberPerson))
        {
            users.Add(new User
            {
                Id = nextId++,
                PersonId = memberPerson.Id,
                Goal = Goal.MuscleGain,
                EstimatedCaloriesIntake = 2500,
                CreatedAt = memberPerson.CreatedAt
            });
        }
        
        // Random Users
        foreach (var person in persons.Where(p => p.Username != "coach" && p.Username != "member"))
        {
            users.Add(new User
            {
                Id = nextId++,
                PersonId = person.Id,
                Goal = _faker.PickRandom<Goal>(),
                EstimatedCaloriesIntake = _faker.Random.Bool(0.7f) ? _faker.Random.Int(1800, 3200) : null,
                CreatedAt = person.CreatedAt
            });
        }
        
        return users;
    }
    
    public List<Coach> GenerateCoaches(List<Person> persons)
    {
        var coaches = new List<Coach>();
        var nextId = 1;
        var personDict = persons.ToDictionary(p => p.Username, p => p);
        
        // Demo Coach
        if (personDict.TryGetValue("coach", out var coachPerson))
        {
            coaches.Add(new Coach
            {
                Id = nextId++,
                PersonId = coachPerson.Id,
                Specialization = "Strength & Conditioning",
                YearsOfExperience = 10,
                CreatedAt = coachPerson.CreatedAt
            });
        }
        
        return coaches;
    }
    
    public (List<WorkoutPlan> WorkoutPlans, List<WorkoutDay> WorkoutDays, List<WorkoutDayExercise> WorkoutDayExercises, 
            List<ProgressLog> ProgressLogs, List<WorkoutSession> WorkoutSessions) 
        GenerateWorkoutData(List<User> users, List<Exercise> exercises)
    {
        var allWorkoutPlans = new List<WorkoutPlan>();
        var allWorkoutDays = new List<WorkoutDay>();
        var allWorkoutDayExercises = new List<WorkoutDayExercise>();
        var allProgressLogs = new List<ProgressLog>();
        var allWorkoutSessions = new List<WorkoutSession>();
        
        var planId = 1;
        var workoutDayId = 1;
        var wdeId = 1;
        var logId = 1;
        var sessionId = 1;
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        foreach (var user in users)
        {
            var planCount = _faker.Random.Int(_options.MinWorkoutPlansPerUser, _options.MaxWorkoutPlansPerUser);
            
            for (int p = 0; p < planCount; p++)
            {
                var phase = p + 1;
                var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-(planCount - p) * 2));
                var endDate = startDate.AddMonths(2);
                var isActive = (p == planCount - 1);
                
                // اطمینان از اینکه startDate از امروز بیشتر نباشد
                var finalStartDate = startDate > today ? today : startDate;
                var finalEndDate = isActive ? null : (DateOnly?)(endDate > today ? today : endDate);
                
                var plan = new WorkoutPlan
                {
                    Id = planId++,
                    UserId = user.Id,
                    Phase = phase,
                    SessionsPerWeek = _faker.Random.Int(3, 5),
                    StartDate = finalStartDate,
                    EndDate = finalEndDate,
                    IsActive = isActive,
                    Notes = _faker.Random.Bool(0.3f) ? _faker.Lorem.Sentence() : null,
                    CreatedAt = finalStartDate.ToDateTime(TimeOnly.MinValue)
                };
                allWorkoutPlans.Add(plan);
                
                var dayCount = _faker.Random.Int(_options.MinWorkoutDaysPerPlan, _options.MaxWorkoutDaysPerPlan);
                var daysList = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
                var selectedDays = daysList.OrderBy(_ => Guid.NewGuid()).Take(dayCount).ToList();
                
                foreach (var dayOfWeek in selectedDays)
                {
                    var workoutDay = new WorkoutDay
                    {
                        Id = workoutDayId++,
                        WorkoutPlanId = plan.Id,
                        DayOfWeek = dayOfWeek,
                        TargetMuscles = (MuscleGroup)_faker.Random.Int(1, 63),
                        DurationMinutes = _faker.Random.Int(45, 90),
                        Intensity = _faker.PickRandom<Intensity>(),
                        Notes = _faker.Random.Bool(0.2f) ? _faker.Lorem.Sentence() : null,
                        CreatedAt = plan.CreatedAt
                    };
                    allWorkoutDays.Add(workoutDay);
                    
                    var exerciseCount = _faker.Random.Int(_options.MinExercisesPerDay, _options.MaxExercisesPerDay);
                    var selectedExercises = exercises.OrderBy(_ => Guid.NewGuid()).Take(exerciseCount).ToList();
                    
                    foreach (var exercise in selectedExercises)
                    {
                        var wde = new WorkoutDayExercise
                        {
                            Id = wdeId++,
                            WorkoutDayId = workoutDay.Id,
                            ExerciseId = exercise.Id,
                            Sets = _faker.Random.Int(3, 5),
                            Reps = $"{_faker.Random.Int(8, 12)},{_faker.Random.Int(8, 12)},{_faker.Random.Int(8, 12)}",
                            RestSeconds = _faker.Random.Int(45, 90),
                            Notes = _faker.Random.Bool(0.15f) ? _faker.Lorem.Sentence() : null,
                            CreatedAt = plan.CreatedAt
                        };
                        allWorkoutDayExercises.Add(wde);
                    }
                    
                    var sessionCount = _faker.Random.Int(4, 8);
                    for (int s = 0; s < sessionCount; s++)
                    {
                        var sessionDate = plan.StartDate.AddDays(s * 7 + (int)dayOfWeek);
                        // اطمینان از اینکه sessionDate از امروز بیشتر نباشد
                        if (sessionDate > today) continue;
                        
                        var session = new WorkoutSession
                        {
                            Id = sessionId++,
                            WorkoutDayId = workoutDay.Id,
                            ActualDate = sessionDate,
                            ActualDurationMinutes = workoutDay.DurationMinutes + _faker.Random.Int(-10, 15),
                            Feeling = _faker.Random.Bool(0.7f) ? _faker.PickRandom(new[] { "Great!", "Tired", "Energetic", "Good session" }) : null,
                            CreatedAt = DateTime.UtcNow
                        };
                        allWorkoutSessions.Add(session);
                    }
                }
                
                var logCount = _faker.Random.Int(3, 8);
                var currentWeight = user.Person?.Weight ?? 75f;
                var isWeightLoss = user.Goal == Goal.FatLoss;
                
                for (int l = 0; l < logCount; l++)
                {
                    var logDate = plan.StartDate.AddDays(l * 14);
                    // اطمینان از اینکه logDate از امروز بیشتر نباشد
                    if (logDate > today) continue;
                    
                    var change = isWeightLoss ? -_faker.Random.Float(0.2f, 0.8f) : _faker.Random.Float(-0.2f, 0.5f);
                    currentWeight += change;
                    currentWeight = Math.Clamp(currentWeight, 45f, 130f);
                    
                    var log = new ProgressLog
                    {
                        Id = logId++,
                        UserId = user.Id,
                        WorkoutPlanId = plan.Id,
                        LogDate = logDate,
                        Weight = (float)Math.Round(currentWeight, 1),
                        BodyFatPercentage = _faker.Random.Bool(0.5f) ? (float?)Math.Round(_faker.Random.Float(8f, 30f), 1) : null,
                        Notes = _faker.Random.Bool(0.2f) ? _faker.Lorem.Sentence() : null,
                        CreatedAt = DateTime.UtcNow
                    };
                    allProgressLogs.Add(log);
                }
            }
        }
        
        return (allWorkoutPlans, allWorkoutDays, allWorkoutDayExercises, allProgressLogs, allWorkoutSessions);
    }
    
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
}