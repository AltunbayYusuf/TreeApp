using System.Net;
using IntergratieProject.UI.MVC.Tests.intergrationTests.Config;

namespace IntergratieProject.UI.MVC.Tests.intergrationTests;

public class ProjectIntegrationTests : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private readonly ExtendedWebApplicationFactory<Program> _factory;


    public ProjectIntegrationTests(ExtendedWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Project_Index_WithValidProjectId_ReturnsSuccess()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("Project/Index?projectId=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}