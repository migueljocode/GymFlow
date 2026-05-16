#nullable disable

using GymFlow.Tests.Web.Pages.TestBase;
using GymFlow.Web.Pages.Progress;
using GymFlow.Web.Services;
using GymFlow.Models.DTOs.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymFlow.Tests.Web.Pages.Progress;

public class AddPageTest : PageModelTestFixture
{
    private readonly Mock<ApiClient> _mockApiClient;
    private readonly AddModel _pageModel;

    public AddPageTest()
    {
        _mockApiClient = new Mock<ApiClient>(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<ILogger<ApiClient>>(),
            Mock.Of<IConfiguration>(),
            Mock.Of<IHttpContextAccessor>())
        { CallBase = true };

        _pageModel = CreatePageModel<AddModel>(_mockApiClient.Object);
    }

    #region OnPostAsync

    [Fact]
    public async Task OnPostAsync_WhenUserNotLoggedIn_RedirectsToLogin()
    {
        // بدون تنظیم Session

        var result = await _pageModel.OnPostAsync();

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Login", redirectResult.PageName);
        Assert.Equal("لطفاً مجدداً وارد شوید.", _pageModel.ErrorMessage);
        _mockApiClient.Verify(c => c.PostAsync<ProgressLogResponse>(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Theory]
    [InlineData(19)]   // کمتر از 20
    [InlineData(301)]  // بیشتر از 300
    public async Task OnPostAsync_WhenWeightOutOfRange_ReturnsPageWithError(float invalidWeight)
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");
        _pageModel.Weight = invalidWeight;
        _pageModel.LogDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await _pageModel.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal("وزن باید بین ۲۰ تا ۳۰۰ کیلوگرم باشد", _pageModel.ErrorMessage);
        _mockApiClient.Verify(c => c.PostAsync<ProgressLogResponse>(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(51)]
    public async Task OnPostAsync_WhenBodyFatOutOfRange_ReturnsPageWithError(float invalidBodyFat)
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");
        _pageModel.Weight = 80;
        _pageModel.BodyFatPercentage = invalidBodyFat;
        _pageModel.LogDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await _pageModel.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal("درصد چربی باید بین ۳ تا ۵۰ باشد", _pageModel.ErrorMessage);
        _mockApiClient.Verify(c => c.PostAsync<ProgressLogResponse>(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenLogDateInFuture_ReturnsPageWithError()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");
        _pageModel.Weight = 80;
        // استفاده از تاریخ آینده با اختلاف ۲ روز
        _pageModel.LogDate = DateOnly.FromDateTime(DateTime.Now.AddDays(2));

        var result = await _pageModel.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal("تاریخ نمی‌تواند در آینده باشد", _pageModel.ErrorMessage);
        _mockApiClient.Verify(c => c.PostAsync<ProgressLogResponse>(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_SuccessfulLog_ReturnsPageWithSuccessMessageAndResetsFields()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");

        var originalWeight = 75.5f;
        var originalBodyFat = 18;
        var originalNotes = "Feeling good";
        var logDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _pageModel.Weight = originalWeight;
        _pageModel.BodyFatPercentage = originalBodyFat;
        _pageModel.Notes = originalNotes;
        _pageModel.LogDate = logDate;

        var response = new ProgressLogResponse { Id = 10, LogDate = logDate, Weight = originalWeight };
        _mockApiClient.Setup(c => c.PostAsync<ProgressLogResponse>("api/progress/user/1", It.IsAny<CreateProgressLogRequest>()))
            .ReturnsAsync(response);

        var result = await _pageModel.OnPostAsync();

        Assert.IsType<PageResult>(result);
        // انتظار داریم پیام حاوی وزن اصلی باشد، نه وزن بازنشانی شده
        Assert.Equal($"وزن {originalWeight} کیلوگرم با موفقیت ثبت شد! 📊", _pageModel.Message);
        Assert.Equal(0, _pageModel.Weight);
        Assert.Null(_pageModel.BodyFatPercentage);
        Assert.Null(_pageModel.Notes);
        Assert.Null(_pageModel.ErrorMessage);

        _mockApiClient.Verify(c => c.PostAsync<ProgressLogResponse>("api/progress/user/1", It.Is<CreateProgressLogRequest>(req =>
            req.LogDate == logDate &&
            req.Weight == originalWeight &&
            req.BodyFatPercentage == originalBodyFat &&
            req.Notes == originalNotes
        )), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WhenApiReturnsNull_ReturnsPageWithGeneralError()
    {
        SetAuthenticatedUser(_pageModel, userId: 1, username: "testuser");
        _pageModel.Weight = 75.5f;
        _pageModel.LogDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockApiClient.Setup(c => c.PostAsync<ProgressLogResponse>($"api/progress/user/1", It.IsAny<CreateProgressLogRequest>()))
            .ReturnsAsync((ProgressLogResponse)null);

        var result = await _pageModel.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal("خطا در ثبت وزن. لطفاً دوباره تلاش کنید.", _pageModel.ErrorMessage);
        Assert.Null(_pageModel.Message);
    }

    #endregion
}