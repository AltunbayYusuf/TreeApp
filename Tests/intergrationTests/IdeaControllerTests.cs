using System.Net;
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
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var dbContextScope = _factory.CreateDbContextScope<TreeDbContext>();
        var recordCount = dbContextScope.DbContext.Ideas.Count();
        var existingTopic = dbContextScope.DbContext.Topics.First();

        // Act
        var response = client.PostAsync("/Idea/Create",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "Title", "Nieuw test idee" },
                { "Description", "Dit is een integration test idee" },
                { "TopicId", existingTopic.Id.ToString() }
            })).Result;

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Found
        );

        Assert.Equal(recordCount + 1, dbContextScope.DbContext.Ideas.Count());

        var createdIdea = dbContextScope.DbContext.Ideas
            .FirstOrDefault(i => i.Title == "Nieuw test idee");

        Assert.NotNull(createdIdea);
        Assert.Equal("Nieuw test idee", createdIdea.Title);
        Assert.Equal("Dit is een integration test idee", createdIdea.Text);
    }

    [Fact]
    public void Create_Should_Not_Accept_Invalid_TopicId()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var dbContextScope = _factory.CreateDbContextScope<TreeDbContext>();
        var recordCount = dbContextScope.DbContext.Ideas.Count();

        // Act
        var response = client.PostAsync("/Idea/Create",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "Title", "Fout idee" },
                { "Description", "Dit mag niet opgeslagen worden" },
                { "TopicId", "999999" }
            })).Result;

        // Assert
        Assert.Equal(recordCount, dbContextScope.DbContext.Ideas.Count());
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.OK
        );
    }
}