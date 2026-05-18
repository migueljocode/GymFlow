namespace GymFlow.Tests.Web.Pages;

public class PrivacyPageTest : PageModelTestFixture
{
    [Fact]
    public void OnGet_ShouldExecuteWithoutException()
    {
        // Arrange
        var pageModel = CreatePageModel<PrivacyModel>();

        // Act
        var exception = Record.Exception(() => pageModel.OnGet());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var pageModel = CreatePageModel<PrivacyModel>();

        // Assert
        Assert.NotNull(pageModel);
    }
}