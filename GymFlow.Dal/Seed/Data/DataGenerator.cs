using Bogus;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;
using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Dal.Seed.Data;

public static class DataGenerator
{
    public static List<Exercise> GenerateExercises(int count = 30)
    {
        var exerciseNames = new[]
        {
            "Bench Press", "Incline Press", "Decline Press", "Push-ups", "Chest Fly",
            "Pull-ups", "Lat Pulldown", "Barbell Row", "Seated Row", "Deadlift",
            "Squat", "Leg Press", "Leg Extension", "Leg Curl", "Lunges",
            "Overhead Press", "Lateral Raise", "Front Raise", "Face Pull", "Shrugs",
            "Bicep Curl", "Tricep Pushdown", "Hammer Curl", "Skull Crusher", "Dips",
            "Plank", "Russian Twist", "Leg Raise", "Crunches", "Ab Wheel"
        };

        var faker = new Faker();
        return exerciseNames.Select(name => new Exercise
        {
            Name = name,
            PrimaryMuscleGroup = faker.PickRandom<MuscleGroup>(),
            Description = $"{name} - {faker.Lorem.Sentence(3)}",
            CreatedAt = DateTime.UtcNow.AddMonths(-faker.Random.Int(1, 12))
        }).Take(count).ToList();
    }

    public static List<Person> GeneratePersons(int count)
    {
        var faker = new Faker();
        var persons = new List<Person>();
        var nextId = 1;

        // دمو مربی
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
            Weight = 85,
            Height = 182,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        });

        // دمو کاربر
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
            Weight = 75,
            Height = 175,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        });

        // کاربران تصادفی
        for (int i = 0; i < count - 2; i++)
        {
            var gender = faker.PickRandom<Gender>();
            var firstName = faker.Name.FirstName(gender == Gender.Male ? Bogus.DataSets.Name.Gender.Male : Bogus.DataSets.Name.Gender.Female);
            var lastName = faker.Name.LastName();
            var username = $"{firstName.ToLower()}.{lastName.ToLower()}{faker.Random.Int(1, 99)}";

            persons.Add(new Person
            {
                Id = nextId++,
                FirstName = firstName,
                LastName = lastName,
                Username = username,
                Password = "pass123",
                Email = faker.Internet.Email(firstName, lastName),
                Gender = gender,
                Age = faker.Random.Int(18, 55),
                Weight = faker.Random.Float(gender == Gender.Female ? 50 : 65, gender == Gender.Female ? 85 : 110),
                Height = faker.Random.Float(gender == Gender.Female ? 155 : 170, gender == Gender.Female ? 175 : 190),
                BodyType = faker.PickRandom<BodyType>(),
                CreatedAt = faker.Date.Past(1)
            });
        }

        return persons;
    }

    public static List<User> GenerateUsers(List<Person> persons)
    {
        var faker = new Faker();
        var users = new List<User>();

        // دمو کاربر
        var memberPerson = persons.First(p => p.Username == "member");
        users.Add(new User
        {
            PersonId = memberPerson.Id,
            Goal = Goal.MuscleGain,
            EstimatedCaloriesIntake = 2500,
            IsCompetitive = false,
            CreatedAt = memberPerson.CreatedAt  // استفاده از CreatedAt شخص
        });

        // کاربران عادی
        foreach (var person in persons.Where(p => p.Username != "member"))
        {
            users.Add(new User
            {
                PersonId = person.Id,
                Goal = faker.PickRandom<Goal>(),
                EstimatedCaloriesIntake = faker.Random.Bool(0.7f) ? faker.Random.Int(1800, 3200) : null,
                IsCompetitive = faker.Random.Bool(0.15f),
                CreatedAt = person.CreatedAt  // استفاده از CreatedAt شخص
            });
        }

        return users;
    }

    public static List<Coach> GenerateCoaches(List<Person> persons)
    {
        var coaches = new List<Coach>();

        var coachPerson = persons.First(p => p.Username == "coach");
        coaches.Add(new Coach
        {
            PersonId = coachPerson.Id,
            Specialization = "Strength & Conditioning",
            YearsOfExperience = 10,
            CreatedAt = coachPerson.CreatedAt
        });

        return coaches;
    }

    public static List<WorkoutPlan> GenerateWorkoutPlans(List<User> users)
    {
        var faker = new Faker();
        var plans = new List<WorkoutPlan>();

        foreach (var user in users)
        {
            var planCount = faker.Random.Int(1, 3);
            for (int i = 0; i < planCount; i++)
            {
                var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-i * 2));
                plans.Add(new WorkoutPlan
                {
                    UserId = user.Id,
                    Phase = i + 1,
                    SessionsPerWeek = faker.Random.Int(3, 5),
                    StartDate = startDate,
                    EndDate = i == 0 ? null : startDate.AddMonths(2),
                    IsActive = i == 0,
                    Notes = faker.Random.Bool(0.3f) ? faker.Lorem.Sentence() : null,
                    CreatedAt = startDate.ToDateTime(TimeOnly.MinValue)
                });
            }
        }

        return plans;
    }

    public static List<WorkoutDay> GenerateWorkoutDays(List<WorkoutPlan> plans)
    {
        var faker = new Faker();
        var days = new List<WorkoutDay>();

        var weekDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

        foreach (var plan in plans)
        {
            var selectedDays = weekDays.OrderBy(_ => Guid.NewGuid()).Take(plan.SessionsPerWeek);
            foreach (var day in selectedDays)
            {
                days.Add(new WorkoutDay
                {
                    WorkoutPlanId = plan.Id,
                    DayOfWeek = day,
                    TargetMuscles = (MuscleGroup)faker.Random.Int(1, 63),
                    DurationMinutes = faker.Random.Int(45, 90),
                    Intensity = faker.PickRandom<Intensity>(),
                    Notes = faker.Random.Bool(0.2f) ? faker.Lorem.Sentence() : null,
                    CreatedAt = plan.CreatedAt
                });
            }
        }

        return days;
    }

    public static List<WorkoutDayExercise> GenerateWorkoutDayExercises(List<WorkoutDay> days, List<Exercise> exercises)
    {
        var faker = new Faker();
        var wdes = new List<WorkoutDayExercise>();

        foreach (var day in days)
        {
            var exerciseCount = faker.Random.Int(3, 6);
            var selectedExercises = exercises.OrderBy(_ => Guid.NewGuid()).Take(exerciseCount);

            foreach (var ex in selectedExercises)
            {
                wdes.Add(new WorkoutDayExercise
                {
                    WorkoutDayId = day.Id,
                    ExerciseId = ex.Id,
                    Sets = faker.Random.Int(3, 5),
                    Reps = $"{faker.Random.Int(8, 12)},{faker.Random.Int(8, 12)},{faker.Random.Int(8, 12)}",
                    RestSeconds = faker.Random.Int(45, 90),
                    Notes = faker.Random.Bool(0.15f) ? faker.Lorem.Sentence() : null,
                    CreatedAt = day.CreatedAt
                });
            }
        }

        return wdes;
    }

    public static List<ProgressLog> GenerateProgressLogs(List<User> users, List<WorkoutPlan> plans)
    {
        var faker = new Faker();
        var logs = new List<ProgressLog>();

        foreach (var user in users)
        {
            var userPlans = plans.Where(p => p.UserId == user.Id).OrderBy(p => p.StartDate).ToList();
            if (!userPlans.Any()) continue;

            var startWeight = user.Person?.Weight ?? 75;
            var currentWeight = startWeight;
            var logDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3));

            for (int i = 0; i < 12; i++)
            {
                logDate = logDate.AddDays(faker.Random.Int(7, 14));
                var plan = userPlans.LastOrDefault(p => p.StartDate <= logDate);
                var isWeightLoss = user.Goal == Goal.FatLoss;
                var change = isWeightLoss ? faker.Random.Float(-0.8f, -0.2f) : faker.Random.Float(-0.2f, 0.5f);
                currentWeight += change;
                currentWeight = Math.Clamp(currentWeight, 45, 130);

                logs.Add(new ProgressLog
                {
                    UserId = user.Id,
                    WorkoutPlanId = plan?.Id,
                    LogDate = logDate,
                    Weight = (float)Math.Round(currentWeight, 1),
                    BodyFatPercentage = faker.Random.Bool(0.5f) ? (float?)Math.Round(faker.Random.Float(8, 30), 1) : null,
                    Notes = faker.Random.Bool(0.2f) ? faker.Lorem.Sentence() : null,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return logs;
    }

    public static List<WorkoutSession> GenerateWorkoutSessions(List<WorkoutDay> days)
    {
        var faker = new Faker();
        var sessions = new List<WorkoutSession>();
        var feelings = new[] { "Great!", "Tired", "Energetic", "Good session", "Felt strong", "Need more rest" };

        foreach (var day in days)
        {
            var plan = day.WorkoutPlan;
            if (plan == null) continue;

            var currentDate = plan.StartDate;
            while (currentDate <= DateOnly.FromDateTime(DateTime.UtcNow) && sessions.Count < 50)
            {
                var daysUntilTarget = ((int)day.DayOfWeek - (int)currentDate.DayOfWeek + 7) % 7;
                var sessionDate = currentDate.AddDays(daysUntilTarget);
                if (sessionDate > DateOnly.FromDateTime(DateTime.UtcNow)) break;

                if (faker.Random.Bool(0.65f))
                {
                    sessions.Add(new WorkoutSession
                    {
                        WorkoutDayId = day.Id,
                        ActualDate = sessionDate,
                        ActualDurationMinutes = day.DurationMinutes + faker.Random.Int(-10, 15),
                        Feeling = faker.Random.Bool(0.7f) ? faker.PickRandom(feelings) : null,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                currentDate = currentDate.AddDays(7);
            }
        }

        return sessions;
    }
}