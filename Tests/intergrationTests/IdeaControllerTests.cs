using System.Net;
using System.Net.Http.Json;
using IntergratieProject.DAL.Ef;
using IntergratieProject.UI.MVC.Tests.intergrationTests.Config;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntergratieProject.UI.MVC.Tests.intergrationTests;

public class IdeaControllerTests : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private readonly ExtendedWebApplicationFactory<Program> _factory;

    public IdeaControllerTests(ExtendedWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Create_Should_Accept_Valid_Idea()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var dbContextScope = _factory.CreateDbContextScope<TreeDbContext>();
        var recordCount = dbContextScope.DbContext.Ideas.Count();
        var existingTopic = dbContextScope.DbContext.Topics.First();

        var response = client.PostAsJsonAsync("/api/Ideas", new
        {
            Title = "Nieuw test idee",
            Text = "Dit is een integration test idee",
            TopicId = existingTopic.Id
        }).Result;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(recordCount + 1, dbContextScope.DbContext.Ideas.Count());

        var createdIdea = dbContextScope.DbContext.Ideas
            .FirstOrDefault(i => i.Title == "Nieuw test idee");

        Assert.NotNull(createdIdea);
        Assert.Equal("Nieuw test idee", createdIdea.Title);
        Assert.Equal("Dit is een integration test idee", createdIdea.Text);
    }
}