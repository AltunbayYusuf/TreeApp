using System.Net;
using IntergratieProject.UI.MVC.Tests.intergrationTests.Config;

namespace IntergratieProject.UI.MVC.Tests.intergrationTests;

public class SurveyIntegrationTests : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SurveyIntegrationTests(ExtendedWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Survey_Index_WithValidProjectId_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Survey/Index?projectId=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_Survey_Index_WithInvalidProjectId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/Survey/Index?projectId=9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_Survey_Index_ResponseContainsPageContent()
    {
        var response = await _client.GetAsync("/Survey/Index?projectId=1");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(content));
    }
}