using Microsoft.AspNetCore.Mvc;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.DTOs.Requests;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Models.Entities;
using GymFlow.Api.Controllers.Base;

namespace GymFlow.Api.Controllers;

[Tags("Workout Plans")]
public class WorkoutPlansController : ApiControllerBase
{
    private readonly IWorkoutPlanRepository _workoutPlanRepository;
    private readonly IWorkoutDayRepository _workoutDayRepository;
    private readonly IWorkoutSessionRepository _workoutSessionRepository;
    private readonly IUserRepository _userRepository;

    public WorkoutPlansController(
        IWorkoutPlanRepository workoutPlanRepository,
        IWorkoutDayRepository workoutDayRepository,
        IWorkoutSessionRepository workoutSessionRepository,
        IUserRepository userRepository)
    {
        _workoutPlanRepository = workoutPlanRepository;
        _workoutDayRepository = workoutDayRepository;
        _workoutSessionRepository = workoutSessionRepository;
        _userRepository = userRepository;
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        var plans = await _workoutPlanRepository.GetUserWorkoutPlansAsync(userId);
        
        var responses = new List<WorkoutPlanResponse>();
        foreach (var plan in plans)
        {
            responses.Add(await MapToResponseAsync(plan));
        }
        
        return Success<IEnumerable<WorkoutPlanResponse>>(responses);
    }

    [HttpGet("{id:int}/details")]
    public async Task<IActionResult> GetDetailsAsync(int id)
    {
        var plan = await _workoutPlanRepository.GetWorkoutPlanWithDetailsAsync(id);
        if (plan is null)
            return NotFoundResponse("WorkoutPlan", id);
        
        var response = await MapToDetailedResponseAsync(plan);
        return Success<WorkoutPlanResponse>(response);
    }

    [HttpGet("user/{userId:int}/active")]
    public async Task<IActionResult> GetActivePlanAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        var plan = await _workoutPlanRepository.GetActiveWorkoutPlanAsync(userId);
        if (plan is null)
            return Success<string?>(null, "No active workout plan found");
        
        var response = await MapToDetailedResponseAsync(plan);
        return Success<WorkoutPlanResponse>(response);
    }

    [HttpGet("user/{userId:int}/today")]
    public async Task<IActionResult> GetTodaysWorkoutAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        var today = DateTime.UtcNow.DayOfWeek;
        var activePlan = await _workoutPlanRepository.GetActiveWorkoutPlanAsync(userId);
        
        if (activePlan is null)
            return Error("No active workout plan found", 404);
        
        var workoutDays = await _workoutDayRepository.GetWorkoutDaysByPlanAsync(activePlan.Id);
        var todayWorkout = workoutDays.FirstOrDefault(wd => wd.DayOfWeek == today);
        
        if (todayWorkout is null)
            return Success<string?>(null, "No workout scheduled for today");
        
        var fullDay = await _workoutDayRepository.GetWorkoutDayWithExercisesAsync(todayWorkout.Id);
        
        var alreadyCompleted = await _workoutSessionRepository.HasUserCompletedWorkoutDayAsync(
            todayWorkout.Id, DateOnly.FromDateTime(DateTime.UtcNow));
        
        var response = new
        {
            workoutDay = fullDay,
            isCompleted = alreadyCompleted,
            scheduledDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        
        return Success<object>(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateWorkoutPlanRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();
        
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
            return NotFoundResponse("User", request.UserId);
        
        await _workoutPlanRepository.DeactivateAllUserPlansAsync(request.UserId);
        
        var plan = new WorkoutPlan
        {
            UserId = request.UserId,
            Phase = request.Phase,
            SessionsPerWeek = request.SessionsPerWeek,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true,
            Notes = request.Notes
        };
        
        var created = await _workoutPlanRepository.AddAsync(plan);
        var response = await MapToResponseAsync(created);
        
        return CreatedResponse<WorkoutPlanResponse>(response, "Workout plan created successfully");
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateWorkoutPlanRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();
        
        var plan = await _workoutPlanRepository.GetByIdAsync(id);
        if (plan is null)
            return NotFoundResponse("WorkoutPlan", id);
        
        if (request.Phase.HasValue) plan.Phase = request.Phase.Value;
        if (request.SessionsPerWeek.HasValue) plan.SessionsPerWeek = request.SessionsPerWeek.Value;
        if (request.StartDate.HasValue) plan.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) plan.EndDate = request.EndDate;
        if (request.IsActive.HasValue) plan.IsActive = request.IsActive.Value;
        if (request.Notes is not null) plan.Notes = request.Notes;
        
        var updated = await _workoutPlanRepository.UpdateAsync(plan);
        var response = await MapToResponseAsync(updated);
        
        return Success<WorkoutPlanResponse>(response, "Workout plan updated successfully");
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> ActivateAsync(int id)
    {
        var plan = await _workoutPlanRepository.GetByIdAsync(id);
        if (plan is null)
            return NotFoundResponse("WorkoutPlan", id);
        
        await _workoutPlanRepository.DeactivateAllUserPlansAsync(plan.UserId);
        await _workoutPlanRepository.ActivateWorkoutPlanAsync(id);
        
        return Success<string?>(null, "Workout plan activated successfully");
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateAsync(int id)
    {
        var plan = await _workoutPlanRepository.GetByIdAsync(id);
        if (plan is null)
            return NotFoundResponse("WorkoutPlan", id);
        
        plan.IsActive = false;
        await _workoutPlanRepository.UpdateAsync(plan);
        
        return Success<string?>(null, "Workout plan deactivated successfully");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var plan = await _workoutPlanRepository.GetByIdAsync(id);
        if (plan is null)
            return NotFoundResponse("WorkoutPlan", id);
        
        await _workoutPlanRepository.SoftDeleteAsync(id);
        return Success<string?>(null, "Workout plan deleted successfully");
    }

    private async Task<WorkoutPlanResponse> MapToResponseAsync(WorkoutPlan plan)
    {
        var user = await _userRepository.GetByIdAsync(plan.UserId);
        
        return new WorkoutPlanResponse
        {
            Id = plan.Id,
            UserId = plan.UserId,
            UserName = user != null ? $"{user.FirstName} {user.LastName}" : string.Empty,
            Phase = plan.Phase,
            SessionsPerWeek = plan.SessionsPerWeek,
            StartDate = plan.StartDate,
            EndDate = plan.EndDate,
            IsActive = plan.IsActive,
            Notes = plan.Notes,
            CreatedAt = plan.CreatedAt,
            CompletedSessionsCount = 0,
            TotalSessionsCount = plan.SessionsPerWeek * 8,
            AverageWeightDuringPlan = null
        };
    }

    private async Task<WorkoutPlanResponse> MapToDetailedResponseAsync(WorkoutPlan plan)
    {
        var response = await MapToResponseAsync(plan);
        
        if (plan.WorkoutDays is not null && plan.WorkoutDays.Any())
        {
            var workoutDays = new List<GymFlow.Models.DTOs.Responses.WorkoutDayResponse>();
            
            foreach (var day in plan.WorkoutDays)
            {
                var dayResponse = new GymFlow.Models.DTOs.Responses.WorkoutDayResponse
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
                    var exercises = new List<GymFlow.Models.DTOs.Responses.WorkoutDayExerciseResponse>();
                    foreach (var wde in day.WorkoutDayExercises)
                    {
                        exercises.Add(new GymFlow.Models.DTOs.Responses.WorkoutDayExerciseResponse
                        {
                            Id = wde.Id,
                            ExerciseId = wde.ExerciseId,
                            ExerciseName = wde.Exercise?.Name ?? "Unknown",
                            MuscleGroup = wde.Exercise?.PrimaryMuscleGroup.ToString() ?? "Unknown",
                            Sets = wde.Sets,
                            Reps = wde.Reps,
                            RestSeconds = wde.RestSeconds,
                            Notes = wde.Notes
                        });
                    }
                    dayResponse.Exercises = exercises;
                }
                
                workoutDays.Add(dayResponse);
            }
            
            response.WorkoutDays = workoutDays;
        }
        
        return response;
    }
}