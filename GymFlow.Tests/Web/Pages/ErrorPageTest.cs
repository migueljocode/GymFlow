using System.Diagnostics;
using GymFlow.Tests.Web.Pages.TestBase;
using GymFlow.Web.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Xunit;

namespace GymFlow.Tests.Web.Pages;

public class ErrorPageTest : PageModelTestFixture
{
    [Fact]
    public void OnGet_WhenActivityCurrentHasId_SetsRequestIdToActivityId()
    {
        // Arrange
        var pageModel = CreatePageModel<ErrorModel>();
        var activity = new Activity("Test").Start();
        var expectedId = activity.Id;

        // Act
        pageModel.OnGet();

        // Assert
        Assert.Equal(expectedId, pageModel.RequestId);
        Assert.True(pageModel.ShowRequestId);
        activity.Stop();
    }

    [Fact]
    public void OnGet_WhenActivityCurrentIsNull_SetsRequestIdToTraceIdentifier()
    {
        // Arrange
        var pageModel = CreatePageModel<ErrorModel>();
        var traceIdentifier = "test-trace-id";
        pageModel.HttpContext.TraceIdentifier = traceIdentifier;

        // Act
        pageModel.OnGet();

        // Assert
        Assert.Equal(traceIdentifier, pageModel.RequestId);
        Assert.True(pageModel.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_WhenRequestIdIsNull_ReturnsFalse()
    {
        // Arrange
        var pageModel = CreatePageModel<ErrorModel>();
        pageModel.RequestId = null;

        // Act & Assert
        Assert.False(pageModel.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_WhenRequestIdIsEmpty_ReturnsFalse()
    {
        // Arrange
        var pageModel = CreatePageModel<ErrorModel>();
        pageModel.RequestId = "";

        // Act & Assert
        Assert.False(pageModel.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_WhenRequestIdHasValue_ReturnsTrue()
    {
        // Arrange
        var pageModel = CreatePageModel<ErrorModel>();
        pageModel.RequestId = "some-id";

        // Act & Assert
        Assert.True(pageModel.ShowRequestId);
    }
}