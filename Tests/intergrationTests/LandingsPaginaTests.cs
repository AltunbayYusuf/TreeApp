using System.Net;
using IntergratieProject.UI.MVC.Tests.intergrationTests.Config;

namespace IntergratieProject.UI.MVC.Tests.intergrationTests;

public class ProjectIntegrationTests : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProjectIntegrationTests(ExtendedWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Project_Index_WithValidProjectId_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Project/Index?id=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task Get_Project_Index_WithoutProjectId_ReturnsSuccessOrRedirect()
    {
        var response = await _client.GetAsync("/Project/Index");

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found
        );
    }

    [Fact]
    public async Task Get_Api_Project_WithInvalidId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/projects/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}