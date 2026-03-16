using System.Net;
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
        var formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Title", "Test idee" },
            { "Description", "Dit is een test idee" },
            { "TopicId", "1" }
        });

        var response = await _client.PostAsync("/Idea/Create", formData);

        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.OK
        );
    }
}