using System.Net;
using System.Net.Http.Json;
using IntergratieProject.UI.MVC.Tests.intergrationTests.Config;

namespace IntergratieProject.UI.MVC.Tests.intergrationTests;

public class IdeaPostIntegrationTests : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IdeaPostIntegrationTests(ExtendedWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_Idea_Create_WithValidInput_ReturnsRedirect()
    {
        var body = new
        {
            Title = "Test idee",
            Text = "Dit is een test idee",
            TopicId = 1
        };

        var response = await _client.PostAsJsonAsync("/api/Ideas", body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    }
    
    
    
    
    
    
}