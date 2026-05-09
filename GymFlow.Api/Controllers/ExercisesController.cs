using Microsoft.AspNetCore.Mvc;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.DTOs.Requests;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;
using GymFlow.Api.Controllers.Base;

namespace GymFlow.Api.Controllers;

/// <summary>
/// Controller for managing exercises library
/// </summary>
[Tags("Exercises")]
public class ExercisesController : ApiControllerBase
{
    private readonly IExerciseRepository _exerciseRepository;

    public ExercisesController(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    /// <summary>
    /// Get all exercises
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] string? muscleGroup = null)
    {
        IEnumerable<Exercise> exercises;
        
        if (!string.IsNullOrEmpty(muscleGroup) && Enum.TryParse<MuscleGroup>(muscleGroup, true, out var group))
        {
            exercises = await _exerciseRepository.GetExercisesByMuscleGroupAsync(group);
        }
        else
        {
            exercises = await _exerciseRepository.GetAllAsync();
        }
        
        var responses = exercises.Select(e => new
        {
            e.Id,
            e.Name,
            MuscleGroup = e.PrimaryMuscleGroup.ToString(),
            e.Description
        });
        
        return Success<object>(responses);
    }

    /// <summary>
    /// Get exercise by ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var exercise = await _exerciseRepository.GetByIdAsync(id);
        if (exercise is null)
            return NotFoundResponse("Exercise", id);
        
        var response = new
        {
            exercise.Id,
            exercise.Name,
            MuscleGroup = exercise.PrimaryMuscleGroup.ToString(),
            exercise.Description
        };
        
        return Success<object>(response);
    }

    /// <summary>
    /// Get most used exercises
    /// </summary>
    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularAsync([FromQuery] int top = 10)
    {
        var exercises = await _exerciseRepository.GetMostUsedExercisesAsync(top);
        
        var responses = exercises.Select(e => new PopularExerciseResponse
        {
            Id = e.Id,
            Name = e.Name,
            MuscleGroup = e.PrimaryMuscleGroup.ToString(),
            UsageCount = 0 // Will be calculated properly
        }).ToList();
        
        return Success<IEnumerable<PopularExerciseResponse>>(responses);
    }

    /// <summary>
    /// Get exercises by muscle group
    /// </summary>
    [HttpGet("muscle-group/{muscleGroup}")]
    public async Task<IActionResult> GetByMuscleGroupAsync(string muscleGroup)
    {
        if (!Enum.TryParse<MuscleGroup>(muscleGroup, true, out var group))
            return Error($"Invalid muscle group: {muscleGroup}", 400);
        
        var exercises = await _exerciseRepository.GetExercisesByMuscleGroupAsync(group);
        
        var responses = exercises.Select(e => new
        {
            e.Id,
            e.Name,
            e.Description
        });
        
        return Success<object>(responses);
    }

    /// <summary>
    /// Create a new exercise
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateExerciseRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();
        
        // Check if exercise already exists
        var exists = await _exerciseRepository.ExerciseExistsAsync(request.Name);
        if (exists)
            return Error($"Exercise '{request.Name}' already exists", 409);
        
        var exercise = new Exercise
        {
            Name = request.Name,
            PrimaryMuscleGroup = request.PrimaryMuscleGroup,
            Description = request.Description
        };
        
        var created = await _exerciseRepository.AddAsync(exercise);
        
        var response = new
        {
            created.Id,
            created.Name,
            MuscleGroup = created.PrimaryMuscleGroup.ToString(),
            created.Description
        };
        
        return CreatedResponse<object>(response, "Exercise created successfully");
    }

    /// <summary>
    /// Update an exercise
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateExerciseRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();
        
        var exercise = await _exerciseRepository.GetByIdAsync(id);
        if (exercise is null)
            return NotFoundResponse("Exercise", id);
        
        if (request.Name is not null) exercise.Name = request.Name;
        if (request.PrimaryMuscleGroup.HasValue) exercise.PrimaryMuscleGroup = request.PrimaryMuscleGroup.Value;
        if (request.Description is not null) exercise.Description = request.Description;
        
        var updated = await _exerciseRepository.UpdateAsync(exercise);
        
        var response = new
        {
            updated.Id,
            updated.Name,
            MuscleGroup = updated.PrimaryMuscleGroup.ToString(),
            updated.Description
        };
        
        return Success<object>(response, "Exercise updated successfully");
    }

    /// <summary>
    /// Delete an exercise (soft delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var exercise = await _exerciseRepository.GetByIdAsync(id);
        if (exercise is null)
            return NotFoundResponse("Exercise", id);
        
        // Check if exercise is used in any workout plan
        var usageCount = await _exerciseRepository.GetExerciseUsageCountAsync(id);
        if (usageCount > 0)
            return Error($"Cannot delete exercise that is used in {usageCount} workout plans", 409);
        
        await _exerciseRepository.SoftDeleteAsync(id);
        return Success<string?>(null, "Exercise deleted successfully");
    }
}