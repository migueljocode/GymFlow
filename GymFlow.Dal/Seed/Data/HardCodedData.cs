using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Dal.Seed.Data;

public static class HardCodedData
{
    public static List<Exercise> GetExercises()
    {
        return new List<Exercise>
        {
            new() { Name = "Bench Press", PrimaryMuscleGroup = MuscleGroup.Chest, Description = "Chest exercise" },
            new() { Name = "Squat", PrimaryMuscleGroup = MuscleGroup.Legs, Description = "Leg exercise" },
            new() { Name = "Deadlift", PrimaryMuscleGroup = MuscleGroup.Back, Description = "Back exercise" },
            new() { Name = "Overhead Press", PrimaryMuscleGroup = MuscleGroup.Shoulders, Description = "Shoulder exercise" },
            new() { Name = "Pull-up", PrimaryMuscleGroup = MuscleGroup.Back, Description = "Back exercise" },
            new() { Name = "Leg Press", PrimaryMuscleGroup = MuscleGroup.Legs, Description = "Leg exercise" },
            new() { Name = "Lat Pulldown", PrimaryMuscleGroup = MuscleGroup.Back, Description = "Back exercise" },
            new() { Name = "Dumbbell Curl", PrimaryMuscleGroup = MuscleGroup.Arms, Description = "Arm exercise" },
            new() { Name = "Tricep Pushdown", PrimaryMuscleGroup = MuscleGroup.Arms, Description = "Arm exercise" },
            new() { Name = "Plank", PrimaryMuscleGroup = MuscleGroup.Core, Description = "Core exercise" }
        };
    }
    
    public static List<Person> GetPersons()
    {
        return new List<Person>
        {
            new() 
            { 
                Id = 1,  // اضافه کردن Id
                FirstName = "Master", LastName = "Coach", Username = "coach", Password = "coach123",
                Email = "coach@gymflow.com", Gender = Gender.Male, Age = 35, Weight = 85, Height = 182,
                BodyType = BodyType.Fit, CreatedAt = DateTime.UtcNow
            },
            new() 
            { 
                Id = 2,  // اضافه کردن Id
                FirstName = "John", LastName = "Doe", Username = "member", Password = "member123",
                Email = "member@gymflow.com", Gender = Gender.Male, Age = 25, Weight = 75, Height = 175,
                BodyType = BodyType.Fit, CreatedAt = DateTime.UtcNow
            },
            new() 
            { 
                Id = 3,  // اضافه کردن Id
                FirstName = "Jane", LastName = "Smith", Username = "janesmith", Password = "password123",
                Email = "jane@test.com", Gender = Gender.Female, Age = 28, Weight = 65, Height = 165,
                BodyType = BodyType.Fit, CreatedAt = DateTime.UtcNow
            }
        };
    }
    
    public static List<User> GetUsers(List<Person> persons)
    {
        var nextId = 1;
        var users = new List<User>();
        
        // پیدا کردن شخص member
        var memberPerson = persons.FirstOrDefault(p => p.Username == "member");
        if (memberPerson != null)
        {
            users.Add(new User 
            { 
                Id = nextId++,
                PersonId = memberPerson.Id, 
                Goal = Goal.MuscleGain, 
                EstimatedCaloriesIntake = 2500,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        // پیدا کردن شخص janesmith
        var janePerson = persons.FirstOrDefault(p => p.Username == "janesmith");
        if (janePerson != null)
        {
            users.Add(new User 
            { 
                Id = nextId++,
                PersonId = janePerson.Id, 
                Goal = Goal.FatLoss, 
                EstimatedCaloriesIntake = 1800,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        return users;
    }

    public static List<Coach> GetCoaches(List<Person> persons)
    {
        var personDict = persons.ToDictionary(p => p.Username, p => p);
        
        return new List<Coach>
        {
            new() 
            { 
                PersonId = personDict["coach"].Id, 
                Specialization = "Strength & Conditioning",
                YearsOfExperience = 10,
                CreatedAt = DateTime.UtcNow
            }
        };
    }
    
    public static List<WorkoutPlan> GetWorkoutPlans(List<User> users)
    {
        if (users == null || !users.Any()) return new List<WorkoutPlan>();
        
        // گرفتن اولین کاربر با Id معتبر (غیر صفر)
        var firstUser = users.FirstOrDefault(u => u.Id > 0);
        if (firstUser == null) return new List<WorkoutPlan>();
        
        return new List<WorkoutPlan>
        {
            new() 
            { 
                UserId = firstUser.Id, 
                Phase = 1, 
                SessionsPerWeek = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
    }
    
    public static List<WorkoutDay> GetWorkoutDays(List<WorkoutPlan> plans)
    {
        var firstPlan = plans.FirstOrDefault();
        if (firstPlan == null) return new List<WorkoutDay>();
        
        var nextId = 1;
        
        return new List<WorkoutDay>
        {
            new() 
            { 
                Id = nextId++,
                WorkoutPlanId = firstPlan.Id, 
                DayOfWeek = DayOfWeek.Monday,
                TargetMuscles = MuscleGroup.Chest,
                DurationMinutes = 60,
                Intensity = Intensity.Medium,
                CreatedAt = DateTime.UtcNow
            },
            new() 
            { 
                Id = nextId++,
                WorkoutPlanId = firstPlan.Id, 
                DayOfWeek = DayOfWeek.Wednesday,
                TargetMuscles = MuscleGroup.Back,
                DurationMinutes = 60,
                Intensity = Intensity.Medium,
                CreatedAt = DateTime.UtcNow
            },
            new() 
            { 
                Id = nextId++,
                WorkoutPlanId = firstPlan.Id, 
                DayOfWeek = DayOfWeek.Friday,
                TargetMuscles = MuscleGroup.Legs,
                DurationMinutes = 60,
                Intensity = Intensity.High,
                CreatedAt = DateTime.UtcNow
            }
        };
    }
    
    public static List<WorkoutDayExercise> GetWorkoutDayExercises(List<WorkoutDay> days, List<Exercise> exercises)
    {
        var result = new List<WorkoutDayExercise>();
        var exerciseDict = exercises.ToDictionary(e => e.Name, e => e);
        
        // پیدا کردن روزها با استفاده از DayOfWeek و WorkoutPlanId
        var monday = days.FirstOrDefault(d => d.DayOfWeek == DayOfWeek.Monday);
        var wednesday = days.FirstOrDefault(d => d.DayOfWeek == DayOfWeek.Wednesday);
        var friday = days.FirstOrDefault(d => d.DayOfWeek == DayOfWeek.Friday);
        
        // Monday - Chest
        if (monday != null && exerciseDict.TryGetValue("Bench Press", out var bench))
        {
            result.Add(new WorkoutDayExercise
            {
                WorkoutDayId = monday.Id,
                ExerciseId = bench.Id,
                Sets = 3,
                Reps = "10,10,8",
                RestSeconds = 60,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        // Wednesday - Back
        if (wednesday != null)
        {
            if (exerciseDict.TryGetValue("Deadlift", out var deadlift))
            {
                result.Add(new WorkoutDayExercise
                {
                    WorkoutDayId = wednesday.Id,
                    ExerciseId = deadlift.Id,
                    Sets = 3,
                    Reps = "5,5,5",
                    RestSeconds = 90,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            if (exerciseDict.TryGetValue("Pull-up", out var pullup))
            {
                result.Add(new WorkoutDayExercise
                {
                    WorkoutDayId = wednesday.Id,
                    ExerciseId = pullup.Id,
                    Sets = 3,
                    Reps = "8,8,6",
                    RestSeconds = 60,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        
        // Friday - Legs
        if (friday != null)
        {
            if (exerciseDict.TryGetValue("Squat", out var squat))
            {
                result.Add(new WorkoutDayExercise
                {
                    WorkoutDayId = friday.Id,
                    ExerciseId = squat.Id,
                    Sets = 4,
                    Reps = "10,10,8,8",
                    RestSeconds = 90,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            if (exerciseDict.TryGetValue("Leg Press", out var legPress))
            {
                result.Add(new WorkoutDayExercise
                {
                    WorkoutDayId = friday.Id,
                    ExerciseId = legPress.Id,
                    Sets = 3,
                    Reps = "12,12,10",
                    RestSeconds = 60,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        
        return result;
    }
    
    public static List<ProgressLog> GetProgressLogs(List<User> users, List<WorkoutPlan> plans)
    {
        var firstUser = users.FirstOrDefault();
        var firstPlan = plans.FirstOrDefault();
        
        if (firstUser == null || firstPlan == null) return new List<ProgressLog>();
        
        return new List<ProgressLog>
        {
            new() 
            { 
                UserId = firstUser.Id, 
                WorkoutPlanId = firstPlan.Id,
                LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-14)),
                Weight = 76.5f,
                Notes = "Starting weight",
                CreatedAt = DateTime.UtcNow.AddDays(-14)
            },
            new() 
            { 
                UserId = firstUser.Id, 
                WorkoutPlanId = firstPlan.Id,
                LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
                Weight = 75.8f,
                Notes = "First week progress",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new() 
            { 
                UserId = firstUser.Id, 
                WorkoutPlanId = firstPlan.Id,
                LogDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Weight = 75.2f,
                Notes = "Feeling stronger!",
                CreatedAt = DateTime.UtcNow
            }
        };
    }
    
    public static List<WorkoutSession> GetWorkoutSessions(List<WorkoutDay> days)
    {
        var result = new List<WorkoutSession>();
        
        foreach (var day in days)
        {
            result.Add(new WorkoutSession
            {
                WorkoutDayId = day.Id,
                ActualDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                ActualDurationMinutes = day.DurationMinutes,
                Feeling = "Great workout!",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            });
        }
        
        return result;
    }
}