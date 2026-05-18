namespace GymFlow.Api.Controllers;

[Tags("Workout Days")]
public class WorkoutDaysController : ApiControllerBase
{
    private readonly IWorkoutDayRepository _workoutDayRepository;
    private readonly IWorkoutPlanRepository _workoutPlanRepository;
    private readonly IExerciseRepository _exerciseRepository;

    public WorkoutDaysController(
        IWorkoutDayRepository workoutDayRepository,
        IWorkoutPlanRepository workoutPlanRepository,
        IExerciseRepository exerciseRepository)
    {
        _workoutDayRepository = workoutDayRepository;
        _workoutPlanRepository = workoutPlanRepository;
        _exerciseRepository = exerciseRepository;
    }

    [HttpGet("plan/{planId:int}")]
    public async Task<IActionResult> GetByPlanAsync(int planId)
    {
        var plan = await _workoutPlanRepository.GetByIdAsync(planId);
        if (plan is null)
            return NotFoundResponse("WorkoutPlan", planId);
        
        var days = await _workoutDayRepository.GetWorkoutDaysByPlanAsync(planId);
        
        var responses = days.Select(d => new WorkoutDayResponse
        {
            Id = d.Id,
            DayOfWeek = d.DayOfWeek,
            TargetMuscles = d.TargetMuscles,
            DurationMinutes = d.DurationMinutes,
            Intensity = d.Intensity,
            Notes = d.Notes,
            TimesCompleted = 0
        }).ToList();
        
        return Success<IEnumerable<WorkoutDayResponse>>(responses);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var day = await _workoutDayRepository.GetWorkoutDayWithExercisesAsync(id);
        if (day is null)
            return NotFoundResponse("WorkoutDay", id);
        
        var response = new WorkoutDayResponse
        {
            Id = day.Id,
            DayOfWeek = day.DayOfWeek,
            TargetMuscles = day.TargetMuscles,
            DurationMinutes = day.DurationMinutes,
            Intensity = day.Intensity,
            Notes = day.Notes,
            TimesCompleted = 0
        };
        
        if (day.WorkoutDayExercises is not null && day.WorkoutDayExercises.Any())
        {
            response.Exercises = day.WorkoutDayExercises.Select(wde => new WorkoutDayExerciseResponse
            {
                Id = wde.Id,
                ExerciseId = wde.ExerciseId,
                ExerciseName = wde.Exercise?.Name ?? "Unknown",
                MuscleGroup = wde.Exercise?.PrimaryMuscleGroup.ToString() ?? "Unknown",
                Sets = wde.Sets,
                Reps = wde.Reps,
                RestSeconds = wde.RestSeconds,
                Notes = wde.Notes
            }).ToList();
        }
        
        return Success<WorkoutDayResponse>(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateWorkoutDayRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();
        
        var plan = await _workoutPlanRepository.GetByIdAsync(request.WorkoutPlanId);
        if (plan is null)
            return NotFoundResponse("WorkoutPlan", request.WorkoutPlanId);
        
        var existingDay = await _workoutDayRepository.GetWorkoutDayByWeekdayAndPlanAsync(
            request.WorkoutPlanId, request.DayOfWeek);
        
        if (existingDay is not null)
            return Error($"A workout day for {request.DayOfWeek} already exists in this plan", 409);
        
        var day = new WorkoutDay
        {
            WorkoutPlanId = request.WorkoutPlanId,
            DayOfWeek = request.DayOfWeek,
            TargetMuscles = request.TargetMuscles,
            DurationMinutes = request.DurationMinutes,
            Intensity = request.Intensity,
            Notes = request.Notes
        };
        
        var created = await _workoutDayRepository.AddAsync(day);
        
        var response = new WorkoutDayResponse
        {
            Id = created.Id,
            DayOfWeek = created.DayOfWeek,
            TargetMuscles = created.TargetMuscles,
            DurationMinutes = created.DurationMinutes,
            Intensity = created.Intensity,
            Notes = created.Notes
        };
        
        return CreatedResponse<WorkoutDayResponse>(response, "Workout day created successfully");
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateWorkoutDayRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();
        
        var day = await _workoutDayRepository.GetByIdAsync(id);
        if (day is null)
            return NotFoundResponse("WorkoutDay", id);
        
        if (request.DayOfWeek.HasValue) day.DayOfWeek = request.DayOfWeek.Value;
        if (request.TargetMuscles.HasValue) day.TargetMuscles = request.TargetMuscles.Value;
        if (request.DurationMinutes.HasValue) day.DurationMinutes = request.DurationMinutes.Value;
        if (request.Intensity.HasValue) day.Intensity = request.Intensity.Value;
        if (request.Notes is not null) day.Notes = request.Notes;
        
        var updated = await _workoutDayRepository.UpdateAsync(day);
        
        var response = new WorkoutDayResponse
        {
            Id = updated.Id,
            DayOfWeek = updated.DayOfWeek,
            TargetMuscles = updated.TargetMuscles,
            DurationMinutes = updated.DurationMinutes,
            Intensity = updated.Intensity,
            Notes = updated.Notes
        };
        
        return Success<WorkoutDayResponse>(response, "Workout day updated successfully");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var day = await _workoutDayRepository.GetByIdAsync(id);
        if (day is null)
            return NotFoundResponse("WorkoutDay", id);
        
        await _workoutDayRepository.SoftDeleteAsync(id);
        return Success<string?>(null, "Workout day deleted successfully");
    }
}