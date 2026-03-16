using System.Net;
using IntergratieProject.DAL.Ef;
using IntergratieProject.UI.MVC.Tests.intergrationTests.Config;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntergratieProject.UI.MVC.Tests.intergrationTests;

public class SurveyControllerTests : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private readonly ExtendedWebApplicationFactory<Program> _factory;

    public SurveyControllerTests(ExtendedWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Submit_Should_Accept_Valid_Survey_Answers()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var dbContextScope = _factory.CreateDbContextScope<TreeDbContext>();
        var recordCount = dbContextScope.DbContext.Answers.Count();

        var existingProject = dbContextScope.DbContext.Projects.First();
        var firstQuestion = dbContextScope.DbContext.Questions.First();

        // Act
        var response = client.PostAsync("/Survey/Submit",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "ProjectId", existingProject.Id.ToString() },
                { "Answers[0].QuestionId", firstQuestion.Id.ToString() },
                { "Answers[0].Text", "Mijn testantwoord" }
            })).Result;

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Found
        );

        Assert.True(dbContextScope.DbContext.Answers.Count() >= recordCount + 1);
    }

    [Fact]
    public void Submit_Should_Not_Accept_Invalid_ProjectId()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var dbContextScope = _factory.CreateDbContextScope<TreeDbContext>();
        var recordCount = dbContextScope.DbContext.Answers.Count();
        var firstQuestion = dbContextScope.DbContext.Questions.First();

        // Act
        var response = client.PostAsync("/Survey/Submit",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "ProjectId", "999999" },
                { "Answers[0].QuestionId", firstQuestion.Id.ToString() },
                { "Answers[0].Text", "Mijn testantwoord" }
            })).Result;

        // Assert
        Assert.Equal(recordCount, dbContextScope.DbContext.Answers.Count());
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.OK
        );
    }
}