using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Dal.Seed.Generators;

/// <summary>
/// Generator for creating ProgressLog and WorkoutSession entities
/// </summary>
public class ProgressGenerator
{
    private readonly Faker _faker;
    private readonly SeedOptions _options;
    private int _logId;
    private int _sessionId;
    
    public ProgressGenerator(SeedOptions options, int startLogId = 1, int startSessionId = 1)
    {
        _options = options;
        _logId = startLogId;
        _sessionId = startSessionId;
        _faker = new Faker("en");
        Randomizer.Seed = new Random(_options.RandomSeed ?? 42);
    }
    
    public List<ProgressLog> GenerateProgressLogs(User user, WorkoutPlan plan, Person person)
    {
        var logs = new List<ProgressLog>();
        var logCount = _faker.Random.Int(_options.MinProgressLogsPerUser / 2, _options.MaxProgressLogsPerUser / 2);
        
        var startDate = plan.StartDate;
        var endDate = plan.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var dateRange = endDate.DayNumber - startDate.DayNumber;
        
        if (dateRange <= 0) return logs;
        
        var currentWeight = person?.Weight ?? 75f;
        var usedDates = new HashSet<DateOnly>();
        var isWeightLossPlan = user.Goal == Goal.FatLoss;
        
        for (int i = 0; i < logCount && usedDates.Count < logCount; i++)
        {
            DateOnly logDate;
            int attempts = 0;
            do
            {
                var daysOffset = _faker.Random.Int(3, Math.Max(7, dateRange / logCount));
                logDate = startDate.AddDays(daysOffset);
                attempts++;
                if (attempts > 50) break;
            } while (logDate > endDate || usedDates.Contains(logDate));
            
            if (logDate > endDate || usedDates.Contains(logDate)) continue;
            usedDates.Add(logDate);
            
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
                CreatedAt = logDate.ToDateTime(TimeOnly.MinValue)
            };
            
            logs.Add(log);
        }
        
        return logs.OrderBy(l => l.LogDate).ToList();
    }
    
    public List<WorkoutSession> GenerateWorkoutSessions(WorkoutDay workoutDay, WorkoutPlan plan)
    {
        var sessions = new List<WorkoutSession>();
        var startDate = plan.StartDate;
        var endDate = plan.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        
        var currentDate = startDate;
        var daysUntilTarget = ((int)workoutDay.DayOfWeek - (int)currentDate.DayOfWeek + 7) % 7;
        currentDate = currentDate.AddDays(daysUntilTarget);
        
        var feelings = new[] { "Energetic!  Did PR on bench", "Good session, felt strong", 
                               "Tired today, went lighter", "Great pump! ", 
                               "Knee slightly sore, skipped squats", "Amazing energy!",
                               "Decent workout, need more sleep", "Personal best on deadlift! ",
                               "Felt weak, took a deload day", "Crushed it! " };
        
        while (currentDate <= endDate)
        {
            var completed = Random.Shared.NextDouble() < _options.WorkoutSessionCompletionRate;
            
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
                    CreatedAt = currentDate.ToDateTime(TimeOnly.MinValue).AddHours(_faker.Random.Int(6, 20))
                };
                sessions.Add(session);
            }
            
            currentDate = currentDate.AddDays(7);
        }
        
        return sessions;
    }
    
    private string GetProgressNote(float weight, Goal goal) => goal switch
    {
        Goal.FatLoss when weight < 70 => "Finally seeing abs definition!",
        Goal.MuscleGain when weight > 80 => "Arms feel fuller, vein visibility increasing",
        Goal.Fitness => "Energy levels are great, recovery improving",
        _ => "Consistent progress week over week"
    };
    
    public int CurrentLogId => _logId;
    public int CurrentSessionId => _sessionId;
}