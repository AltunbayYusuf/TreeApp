using System.Net;
using IntergratieProject.DAL.Ef;
using IntergratieProject.UI.MVC.Tests.intergrationTests.Config;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntergratieProject.UI.MVC.Tests.intergrationTests;

public class ReactionControllerTests : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private readonly ExtendedWebApplicationFactory<Program> _factory;

    public ReactionControllerTests(ExtendedWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Create_Should_Accept_Text_Reaction()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var dbContextScope = _factory.CreateDbContextScope<TreeDbContext>();
        var recordCount = dbContextScope.DbContext.Reactions.Count();
        var existingIdea = dbContextScope.DbContext.Ideas.First();

        // Act
        var response = client.PostAsync("/api/Reactions",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "IdeaId", existingIdea.Id.ToString() },
                { "Text", "Dit is een testreactie" }
            })).Result;

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Found
        );

        Assert.Equal(recordCount + 1, dbContextScope.DbContext.Reactions.Count());

        var createdReaction = dbContextScope.DbContext.Reactions
            .FirstOrDefault(r => r.Text == "Dit is een testreactie");

        Assert.NotNull(createdReaction);
        Assert.Equal("Dit is een testreactie", createdReaction.Text);
    }

    [Fact]
    public void Create_Should_Accept_Emoji_Reaction()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var dbContextScope = _factory.CreateDbContextScope<TreeDbContext>();
        var recordCount = dbContextScope.DbContext.Reactions.Count();
        var existingIdea = dbContextScope.DbContext.Ideas.First();

        // Act
        var response = client.PostAsync("/api/Reactions",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "IdeaId", existingIdea.Id.ToString() },
                { "Emoji", "👍" }
            })).Result;

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Found
        );

        Assert.Equal(recordCount + 1, dbContextScope.DbContext.Reactions.Count());
    }
}