#nullable disable

using GymFlow.Tests.Web.Pages.TestBase;
using GymFlow.Web.Pages.WorkoutPlans;
using GymFlow.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;

namespace GymFlow.Tests.Web.Pages.WorkoutPlans;

public class AddExercisesPageTest : PageModelTestFixture
{
    private readonly Mock<ApiClient> _mockApiClient;
    private readonly AddExercisesModel _pageModel;

    public AddExercisesPageTest()
    {
        _mockApiClient = new Mock<ApiClient>(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<ILogger<ApiClient>>(),
            Mock.Of<IConfiguration>(),
            Mock.Of<IHttpContextAccessor>())
        { CallBase = true };

        _pageModel = CreatePageModel<AddExercisesModel>(_mockApiClient.Object);
    }

    #region OnGetAsync

    [Fact]
    public async Task OnGetAsync_SetsPropertiesAndLoadsData()
    {
        // Arrange
        var workoutDayId = 10;
        var workoutPlanId = 20;
        var dayOfWeek = "Monday";

        var exercises = new List<ExerciseItem>
        {
            new() { Id = 1, Name = "Bench Press", MuscleGroup = "Chest" },
            new() { Id = 2, Name = "Squat", MuscleGroup = "Legs" }
        };
        var planDetails = new WorkoutPlanDetails
        {
            Id = workoutPlanId,
            WorkoutDays = new List<WorkoutDayDetail>
            {
                new()
                {
                    Id = workoutDayId,
                    TargetMuscles = 5,
                    Intensity = 1,
                    DurationMinutes = 60,
                    Notes = "Test notes",
                    Exercises = new List<ExerciseInDay>
                    {
                        new() { Id = 100, ExerciseName = "Push-up", Sets = 3, Reps = "10,10,8", RestSeconds = 60, Notes = "Warmup" }
                    }
                }
            }
        };

        _mockApiClient.Setup(c => c.GetAsync<List<ExerciseItem>>("api/exercises"))
            .ReturnsAsync(exercises);
        _mockApiClient.Setup(c => c.GetAsync<WorkoutPlanDetails>($"api/workoutplans/{workoutPlanId}/details"))
            .ReturnsAsync(planDetails);

        // Act
        var result = await _pageModel.OnGetAsync(workoutDayId, workoutPlanId, dayOfWeek);

        // Assert
        Assert.Equal(workoutDayId, _pageModel.WorkoutDayId);
        Assert.Equal(workoutPlanId, _pageModel.WorkoutPlanId);
        Assert.Equal(dayOfWeek, _pageModel.DayOfWeek);

        Assert.Equal(5, _pageModel.TargetMuscles);
        Assert.Equal(1, _pageModel.Intensity);
        Assert.Equal(60, _pageModel.DurationMinutes);
        Assert.Equal("Test notes", _pageModel.Notes);

        Assert.Equal(2, _pageModel.ExerciseList.Count);
        Assert.Equal("Bench Press - Chest", _pageModel.ExerciseList[0].Text);
        Assert.Single(_pageModel.ExistingExercises);
        Assert.Equal("Push-up", _pageModel.ExistingExercises[0].ExerciseName);

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnGetAsync_WhenApiReturnsNull_HandlesGracefully()
    {
        _mockApiClient.Setup(c => c.GetAsync<List<ExerciseItem>>("api/exercises"))
            .ReturnsAsync((List<ExerciseItem>)null);
        _mockApiClient.Setup(c => c.GetAsync<WorkoutPlanDetails>("api/workoutplans/1/details"))
            .ReturnsAsync((WorkoutPlanDetails)null);

        var result = await _pageModel.OnGetAsync(1, 1, "Monday");

        Assert.Empty(_pageModel.ExerciseList);
        Assert.Empty(_pageModel.ExistingExercises);
        Assert.IsType<PageResult>(result);
    }

    #endregion

    #region OnPostAsync - Save action

    [Fact]
    public async Task OnPostAsync_WithSaveAction_CallsPutAndRedirects()
    {
        // Arrange
        _pageModel.WorkoutDayId = 5;
        _pageModel.TargetMuscles = 7;
        _pageModel.Intensity = 2;
        _pageModel.DurationMinutes = 75;
        _pageModel.Notes = "Updated notes";

        SetupRequestForm("save");

        _mockApiClient.Setup(c => c.PutAsync($"api/workoutdays/{_pageModel.WorkoutDayId}", It.IsAny<object>()))
            .ReturnsAsync(true);

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        _mockApiClient.Verify(c => c.PutAsync($"api/workoutdays/5", It.Is<object>(o =>
            o != null &&
            (int)o.GetType().GetProperty("TargetMuscles").GetValue(o) == 7 &&
            (int)o.GetType().GetProperty("Intensity").GetValue(o) == 2 &&
            (int)o.GetType().GetProperty("DurationMinutes").GetValue(o) == 75 &&
            (string)o.GetType().GetProperty("Notes").GetValue(o) == "Updated notes"
        )), Times.Once);

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Null(redirectResult.PageName);
        Assert.Equal(_pageModel.WorkoutDayId, redirectResult.RouteValues["workoutDayId"]);
        Assert.Equal(_pageModel.WorkoutPlanId, redirectResult.RouteValues["workoutPlanId"]);
        Assert.Equal(_pageModel.DayOfWeek, redirectResult.RouteValues["dayOfWeek"]);
        Assert.Equal("تغییرات با موفقیت ذخیره شد!", _pageModel.TempData["Message"]);
    }

    #endregion

    #region OnPostAsync - Add exercise action

    [Fact]
    public async Task OnPostAsync_WithAddAction_CallsPostAndRedirects()
    {
        // Arrange
        _pageModel.WorkoutDayId = 10;
        _pageModel.NewExerciseId = 7;
        _pageModel.NewSets = 4;
        _pageModel.NewReps = "12,10,8";
        _pageModel.NewRestSeconds = 90;
        _pageModel.NewNotes = "Focus on form";

        SetupRequestForm("add");

        _mockApiClient.Setup(c => c.PostAsync<object>("api/workoutdayexercises", It.IsAny<object>()))
            .ReturnsAsync(new { Id = 55 });

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        _mockApiClient.Verify(c => c.PostAsync<object>("api/workoutdayexercises", It.Is<object>(o =>
            o != null &&
            (int)o.GetType().GetProperty("WorkoutDayId").GetValue(o) == 10 &&
            (int)o.GetType().GetProperty("ExerciseId").GetValue(o) == 7 &&
            (int)o.GetType().GetProperty("Sets").GetValue(o) == 4 &&
            (string)o.GetType().GetProperty("Reps").GetValue(o) == "12,10,8" &&
            (int)o.GetType().GetProperty("RestSeconds").GetValue(o) == 90 &&
            (string)o.GetType().GetProperty("Notes").GetValue(o) == "Focus on form"
        )), Times.Once);

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("حرکت با موفقیت اضافه شد!", _pageModel.TempData["Message"]);
        Assert.Equal(_pageModel.WorkoutDayId, redirectResult.RouteValues["workoutDayId"]);
    }

    #endregion

    #region OnPostAsync - Update exercise action

    [Fact]
    public async Task OnPostAsync_WithUpdateAction_CallsPutAndRedirects()
    {
        // Arrange
        _pageModel.WorkoutDayId = 10;
        _pageModel.WorkoutPlanId = 20;
        _pageModel.DayOfWeek = "Tuesday";

        var exerciseId = 123;
        var sets = 5;
        var reps = "15,12,10";
        var restSeconds = 120;

        SetupRequestForm($"update_{exerciseId}", additionalFields: new Dictionary<string, string>
        {
            ["Sets"] = sets.ToString(),
            ["Reps"] = reps,
            ["RestSeconds"] = restSeconds.ToString()
        });

        _mockApiClient.Setup(c => c.PutAsync($"api/workoutdayexercises/{exerciseId}", It.IsAny<object>()))
            .ReturnsAsync(true);

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        _mockApiClient.Verify(c => c.PutAsync($"api/workoutdayexercises/{exerciseId}", It.Is<object>(o =>
            o != null &&
            (int)o.GetType().GetProperty("Sets").GetValue(o) == sets &&
            (string)o.GetType().GetProperty("Reps").GetValue(o) == reps &&
            (int)o.GetType().GetProperty("RestSeconds").GetValue(o) == restSeconds
        )), Times.Once);

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("حرکت با موفقیت به‌روز شد!", _pageModel.TempData["Message"]);
    }

    #endregion

    #region OnPostAsync - Delete exercise action

    [Fact]
    public async Task OnPostAsync_WithDeleteAction_CallsDeleteAndRedirects()
    {
        // Arrange
        var exerciseId = 456;
        SetupRequestForm($"delete_{exerciseId}");

        _mockApiClient.Setup(c => c.DeleteAsync($"api/workoutdayexercises/{exerciseId}"))
            .ReturnsAsync(true);

        // Act
        var result = await _pageModel.OnPostAsync();

        // Assert
        _mockApiClient.Verify(c => c.DeleteAsync($"api/workoutdayexercises/{exerciseId}"), Times.Once);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("حرکت با موفقیت حذف شد!", _pageModel.TempData["Message"]);
    }

    #endregion

    #region Helper methods for Request.Form

    private void SetupRequestForm(string action, Dictionary<string, string> additionalFields = null)
    {
        var formDict = new Dictionary<string, StringValues>
        {
            ["action"] = action
        };
        if (additionalFields != null)
        {
            foreach (var kv in additionalFields)
                formDict[kv.Key] = kv.Value;
        }
        var formCollection = new FormCollection(formDict);
        _pageModel.PageContext.HttpContext.Request.Form = formCollection;
    }

    #endregion
}