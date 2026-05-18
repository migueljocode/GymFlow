global using GymFlow.Models.DTOs.Requests;
global using GymFlow.Models.DTOs.Responses;

namespace GymFlow.Api.Controllers;

[Tags("Workout Day Exercises")]
public class WorkoutDayExercisesController : ApiControllerBase
{
    private readonly IWorkoutDayRepository _workoutDayRepository;
    private readonly IExerciseRepository _exerciseRepository;

    public WorkoutDayExercisesController(
        IWorkoutDayRepository workoutDayRepository,
        IExerciseRepository exerciseRepository)
    {
        _workoutDayRepository = workoutDayRepository;
        _exerciseRepository = exerciseRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateWorkoutDayExerciseRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();

        var workoutDay = await _workoutDayRepository.GetByIdAsync(request.WorkoutDayId);
        if (workoutDay is null)
            return NotFoundResponse("WorkoutDay", request.WorkoutDayId);

        var exercise = await _exerciseRepository.GetByIdAsync(request.ExerciseId);
        if (exercise is null)
            return NotFoundResponse("Exercise", request.ExerciseId);

        var existingExercises = await _workoutDayRepository.GetExercisesByDayIdAsync(request.WorkoutDayId);
        if (existingExercises.Any(e => e.ExerciseId == request.ExerciseId))
        {
            return Conflict(new { error = "این حرکت قبلاً به این روز اضافه شده است!" });
        }

        var wde = new WorkoutDayExercise
        {
            WorkoutDayId = request.WorkoutDayId,
            ExerciseId = request.ExerciseId,
            Sets = request.Sets,
            Reps = request.Reps,
            RestSeconds = request.RestSeconds,
            Notes = request.Notes
        };

        await _workoutDayRepository.AddExerciseToDayAsync(wde);
        
        return CreatedResponse<object>(new { id = wde.Id }, "Exercise added successfully");
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateWorkoutDayExerciseRequest request)
    {
        var wde = await _workoutDayRepository.GetExerciseByIdAsync(id);
        if (wde is null)
            return NotFoundResponse("WorkoutDayExercise", id);

        if (request.Sets.HasValue) wde.Sets = request.Sets.Value;
        if (!string.IsNullOrEmpty(request.Reps)) wde.Reps = request.Reps;
        if (request.RestSeconds.HasValue) wde.RestSeconds = request.RestSeconds.Value;
        if (request.Notes is not null) wde.Notes = request.Notes;

        await _workoutDayRepository.UpdateExerciseAsync(wde);
        return Success<object>(null, "Exercise updated successfully");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var wde = await _workoutDayRepository.GetExerciseByIdAsync(id);
        if (wde is null)
            return NotFoundResponse("WorkoutDayExercise", id);

        await _workoutDayRepository.DeleteExerciseAsync(wde);
        return Success<object>(null, "Exercise deleted successfully");
    }
}