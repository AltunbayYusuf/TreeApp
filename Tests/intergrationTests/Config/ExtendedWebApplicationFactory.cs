using IntergratieProject.DAL.Ef;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntergratieProject.UI.MVC.Tests.intergrationTests.Config;

public class ExtendedWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private const string TestConnectionString =
        "Host=localhost;Port=5432;Database=TreeApp;Username=postgres;Password=Student_1234";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<TreeDbContext>>();
            services.RemoveAll<TreeDbContext>();

            services.AddDbContext<TreeDbContext>(options =>
            {
                options.UseNpgsql(TestConnectionString);
            });

            var provider = services.BuildServiceProvider();

            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TreeDbContext>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            DataSeeder.Seed(db);
        });
    }
}