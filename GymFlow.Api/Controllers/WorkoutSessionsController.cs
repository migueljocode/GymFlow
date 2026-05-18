namespace GymFlow.Api.Controllers;

/// <summary>
/// Controller for logging and retrieving workout sessions
/// </summary>
[Tags("Workout Sessions")]
public class WorkoutSessionsController : ApiControllerBase
{
    private readonly IWorkoutSessionRepository _workoutSessionRepository;
    private readonly IWorkoutDayRepository _workoutDayRepository;
    private readonly IWorkoutPlanRepository _workoutPlanRepository;
    private readonly IUserRepository _userRepository;

    public WorkoutSessionsController(
        IWorkoutSessionRepository workoutSessionRepository,
        IWorkoutDayRepository workoutDayRepository,
        IWorkoutPlanRepository workoutPlanRepository,
        IUserRepository userRepository)
    {
        _workoutSessionRepository = workoutSessionRepository;
        _workoutDayRepository = workoutDayRepository;
        _workoutPlanRepository = workoutPlanRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get all workout sessions for a user
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
        
        var responses = sessions.Select(s => new WorkoutSessionResponse
        {
            Id = s.Id,
            WorkoutDayId = s.WorkoutDayId,
            WorkoutDayName = s.WorkoutDay?.DayOfWeek.ToString() ?? "Unknown",
            MuscleGroup = s.WorkoutDay?.TargetMuscles.ToString() ?? "Unknown",
            ActualDate = s.ActualDate,
            ActualDurationMinutes = s.ActualDurationMinutes,
            Feeling = s.Feeling,
            CreatedAt = s.CreatedAt
        }).ToList();
        
        return Success<IEnumerable<WorkoutSessionResponse>>(responses);
    }

    /// <summary>
    /// Get workout sessions within a date range
    /// </summary>
    [HttpGet("user/{userId:int}/range")]
    public async Task<IActionResult> GetByDateRangeAsync(
        int userId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        
        var sessions = await _workoutSessionRepository.GetSessionsByDateRangeAsync(userId, start, end);
        
        var responses = sessions.Select(s => new WorkoutSessionResponse
        {
            Id = s.Id,
            WorkoutDayId = s.WorkoutDayId,
            WorkoutDayName = s.WorkoutDay?.DayOfWeek.ToString() ?? "Unknown",
            MuscleGroup = s.WorkoutDay?.TargetMuscles.ToString() ?? "Unknown",
            ActualDate = s.ActualDate,
            ActualDurationMinutes = s.ActualDurationMinutes,
            Feeling = s.Feeling,
            CreatedAt = s.CreatedAt
        }).ToList();
        
        return Success<IEnumerable<WorkoutSessionResponse>>(responses);
    }

    /// <summary>
    /// Get weekly summary for a user
    /// </summary>
    [HttpGet("user/{userId:int}/weekly-summary")]
    public async Task<IActionResult> GetWeeklySummaryAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(6);
        
        var sessions = await _workoutSessionRepository.GetSessionsByDateRangeAsync(userId, startOfWeek, endOfWeek);
        var sessionsList = sessions.ToList();
        
        var sessionsByDay = new Dictionary<string, int>();
        foreach (var day in Enum.GetValues<DayOfWeek>())
        {
            sessionsByDay[day.ToString()] = 0;
        }
        
        foreach (var session in sessionsList)
        {
            var dayName = session.ActualDate.DayOfWeek.ToString();
            sessionsByDay[dayName] = sessionsByDay.GetValueOrDefault(dayName) + 1;
        }
        
        var activePlan = await _workoutPlanRepository.GetActiveWorkoutPlanAsync(userId);
        var expectedSessions = activePlan?.SessionsPerWeek ?? 3;
        var completionPercentage = expectedSessions > 0 
            ? (int)((double)sessionsList.Count / expectedSessions * 100) 
            : 0;
        
        var summary = new WeeklySummaryResponse
        {
            WeekStart = startOfWeek,
            WeekEnd = endOfWeek,
            TotalSessions = sessionsList.Count,
            SessionsByDay = sessionsByDay,
            TotalDurationMinutes = sessionsList.Sum(s => s.ActualDurationMinutes),
            AverageDurationMinutes = sessionsList.Any() ? sessionsList.Average(s => s.ActualDurationMinutes) : 0,
            CompletedPlanPercentage = Math.Min(completionPercentage, 100)
        };
        
        return Success<WeeklySummaryResponse>(summary);
    }

    /// <summary>
    /// Log a completed workout session
    /// </summary>
    [HttpPost("log")]
    public async Task<IActionResult> LogWorkoutAsync([FromBody] LogWorkoutRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();
        
        var workoutDay = await _workoutDayRepository.GetByIdAsync(request.WorkoutDayId);
        if (workoutDay is null)
            return NotFoundResponse("WorkoutDay", request.WorkoutDayId);
        
        // Check if already logged for this date
        var alreadyLogged = await _workoutSessionRepository.HasUserCompletedWorkoutDayAsync(
            request.WorkoutDayId, request.ActualDate);
        
        if (alreadyLogged)
            return Error("Workout already logged for this date", 409);
        
        var session = new WorkoutSession
        {
            WorkoutDayId = request.WorkoutDayId,
            ActualDate = request.ActualDate,
            ActualDurationMinutes = request.ActualDurationMinutes,
            Feeling = request.Feeling
        };
        
        var created = await _workoutSessionRepository.AddAsync(session);
        
        var response = new WorkoutSessionResponse
        {
            Id = created.Id,
            WorkoutDayId = created.WorkoutDayId,
            WorkoutDayName = workoutDay.DayOfWeek.ToString(),
            MuscleGroup = workoutDay.TargetMuscles.ToString(),
            ActualDate = created.ActualDate,
            ActualDurationMinutes = created.ActualDurationMinutes,
            Feeling = created.Feeling,
            CreatedAt = created.CreatedAt
        };
        
        return CreatedResponse<WorkoutSessionResponse>(response, "Workout logged successfully");
    }

    /// <summary>
    /// Get streak information for a user
    /// </summary>
    [HttpGet("user/{userId:int}/streak")]
    public async Task<IActionResult> GetStreakAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
        var sortedSessions = sessions.OrderByDescending(s => s.ActualDate).ToList();
        
        var currentStreak = 0;
        var longestStreak = 0;
        var streakCount = 0;
        var lastDate = DateOnly.MaxValue;
        
        foreach (var session in sortedSessions)
        {
            if (lastDate == DateOnly.MaxValue)
            {
                streakCount = 1;
                lastDate = session.ActualDate;
            }
            else if (session.ActualDate == lastDate.AddDays(-1))
            {
                streakCount++;
                lastDate = session.ActualDate;
            }
            else if (session.ActualDate < lastDate.AddDays(-1))
            {
                longestStreak = Math.Max(longestStreak, streakCount);
                streakCount = 1;
                lastDate = session.ActualDate;
            }
        }
        
        longestStreak = Math.Max(longestStreak, streakCount);
        
        // Check current streak (from today backwards)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentDate = today;
        currentStreak = 0;
        
        while (sortedSessions.Any(s => s.ActualDate == currentDate))
        {
            currentStreak++;
            currentDate = currentDate.AddDays(-1);
        }
        
        var streakInfo = new
        {
            currentStreak,
            longestStreak,
            workoutDays = new
            {
                total = sortedSessions.Count,
                thisWeek = sortedSessions.Count(s => s.ActualDate >= today.AddDays(-(int)today.DayOfWeek)),
                thisMonth = sortedSessions.Count(s => s.ActualDate >= today.AddDays(-30))
            }
        };
        
        return Success<object>(streakInfo);
    }

    /// <summary>
    /// Delete a workout session (soft delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var session = await _workoutSessionRepository.GetByIdAsync(id);
        if (session is null)
            return NotFoundResponse("WorkoutSession", id);
        
        await _workoutSessionRepository.SoftDeleteAsync(id);
        return Success<string?>(null, "Workout session deleted successfully");
    }
}