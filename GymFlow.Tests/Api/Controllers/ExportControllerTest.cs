using GymFlow.Api.Controllers;
using GymFlow.Services.Interfaces;
using GymFlow.Tests.Api.Controllers.TestBase;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GymFlow.Tests.Api.Controllers;

public class ExportControllerTest : ControllerTestFixture
{
    private readonly Mock<IPdfExportService> _mockPdfService;
    private readonly ExportController _controller;

    public ExportControllerTest()
    {
        _mockPdfService = new Mock<IPdfExportService>();
        _controller = CreateController<ExportController>(_mockPdfService.Object);
    }

    #region ExportWorkoutPlanAsync

    [Fact]
    public async Task ExportWorkoutPlanAsync_ShouldCallServiceAndReturnFile()
    {
        // Arrange
        int planId = 5;
        byte[] expectedBytes = new byte[] { 1, 2, 3, 4 };
        _mockPdfService.Setup(s => s.ExportWorkoutPlanToPdfAsync(planId))
            .ReturnsAsync(expectedBytes);

        // Act
        var result = await _controller.ExportWorkoutPlanAsync(planId);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal($"WorkoutPlan_{planId}.pdf", fileResult.FileDownloadName);
        Assert.Equal(expectedBytes, fileResult.FileContents);
        _mockPdfService.Verify(s => s.ExportWorkoutPlanToPdfAsync(planId), Times.Once);
    }

    #endregion

    #region ExportProgressReportAsync

    [Fact]
    public async Task ExportProgressReportAsync_WithoutDates_ShouldCallServiceAndReturnFile()
    {
        // Arrange
        int userId = 10;
        byte[] expectedBytes = new byte[] { 5, 6, 7 };
        _mockPdfService.Setup(s => s.ExportProgressReportToPdfAsync(userId, null, null))
            .ReturnsAsync(expectedBytes);

        // Act
        var result = await _controller.ExportProgressReportAsync(userId);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal($"ProgressReport_User_{userId}.pdf", fileResult.FileDownloadName);
        Assert.Equal(expectedBytes, fileResult.FileContents);
        _mockPdfService.Verify(s => s.ExportProgressReportToPdfAsync(userId, null, null), Times.Once);
    }

    [Fact]
    public async Task ExportProgressReportAsync_WithDates_ShouldCallServiceWithDates()
    {
        // Arrange
        int userId = 10;
        DateOnly fromDate = new DateOnly(2024, 1, 1);
        DateOnly toDate = new DateOnly(2024, 12, 31);
        byte[] expectedBytes = new byte[] { 8, 9 };
        _mockPdfService.Setup(s => s.ExportProgressReportToPdfAsync(userId, fromDate, toDate))
            .ReturnsAsync(expectedBytes);

        // Act
        var result = await _controller.ExportProgressReportAsync(userId, fromDate, toDate);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal($"ProgressReport_User_{userId}.pdf", fileResult.FileDownloadName);
        Assert.Equal(expectedBytes, fileResult.FileContents);
        _mockPdfService.Verify(s => s.ExportProgressReportToPdfAsync(userId, fromDate, toDate), Times.Once);
    }

    #endregion

    #region ExportWeeklySummaryAsync

    [Fact]
    public async Task ExportWeeklySummaryAsync_WithoutWeekStart_ShouldCallServiceAndReturnFile()
    {
        // Arrange
        int userId = 7;
        byte[] expectedBytes = new byte[] { 10, 11, 12 };
        _mockPdfService.Setup(s => s.ExportWeeklySummaryToPdfAsync(userId, null))
            .ReturnsAsync(expectedBytes);

        // Act
        var result = await _controller.ExportWeeklySummaryAsync(userId);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal($"WeeklySummary_User_{userId}.pdf", fileResult.FileDownloadName);
        Assert.Equal(expectedBytes, fileResult.FileContents);
        _mockPdfService.Verify(s => s.ExportWeeklySummaryToPdfAsync(userId, null), Times.Once);
    }

    [Fact]
    public async Task ExportWeeklySummaryAsync_WithWeekStart_ShouldCallServiceWithWeekStart()
    {
        // Arrange
        int userId = 7;
        DateOnly weekStart = new DateOnly(2024, 10, 14);
        byte[] expectedBytes = new byte[] { 13, 14 };
        _mockPdfService.Setup(s => s.ExportWeeklySummaryToPdfAsync(userId, weekStart))
            .ReturnsAsync(expectedBytes);

        // Act
        var result = await _controller.ExportWeeklySummaryAsync(userId, weekStart);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal($"WeeklySummary_User_{userId}.pdf", fileResult.FileDownloadName);
        Assert.Equal(expectedBytes, fileResult.FileContents);
        _mockPdfService.Verify(s => s.ExportWeeklySummaryToPdfAsync(userId, weekStart), Times.Once);
    }

    #endregion

    #region ExportCertificateAsync

    [Fact]
    public async Task ExportCertificateAsync_ShouldCallServiceAndReturnFile()
    {
        // Arrange
        int userId = 3;
        byte[] expectedBytes = new byte[] { 15, 16, 17, 18 };
        _mockPdfService.Setup(s => s.ExportAchievementsCertificateAsync(userId))
            .ReturnsAsync(expectedBytes);

        // Act
        var result = await _controller.ExportCertificateAsync(userId);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal($"Certificate_User_{userId}.pdf", fileResult.FileDownloadName);
        Assert.Equal(expectedBytes, fileResult.FileContents);
        _mockPdfService.Verify(s => s.ExportAchievementsCertificateAsync(userId), Times.Once);
    }

    #endregion
}