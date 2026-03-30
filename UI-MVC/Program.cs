using IntergratieProject.BL;
using IntergratieProject.DAL;
using IntergratieProject.DAL.Ef;
using IntergratieProject.Domain.Ai;
using IntergratieProject.Domain.users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vite.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<TreeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<TreeDbContext>();
builder.Services.AddHttpClient<IAiService, GeminiService>();
builder.Services.AddScoped<IManager, Manager>();
builder.Services.AddScoped<IIdeaRepository, IdeaRepository>();
builder.Services.AddViteServices();


builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<TreeDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseViteDevelopmentServer();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TreeDbContext>();

    if (context.CreateDatabase(dropDatabase: true))
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        SeedIdentity(userManager, roleManager);
        DataSeeder.Seed(context);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Project}/{action=Index}/{id=1}");


app.Run();

void SeedIdentity(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    var adminuser = new ApplicationUser
    {
        UserName = "admin@test.com",
        Email = "admin@test.com"
    };
    userManager.CreateAsync(adminuser, "Test123!");
    
    var test = new ApplicationUser
    {
        UserName = "test@test.com",
        Email = "test@test.com"
    };
    userManager.CreateAsync(adminuser, "Test123!");


    
    userManager.AddToRoleAsync(adminuser, "GeneralAdmin").Wait();
    userManager.AddToRoleAsync(adminuser, "SubAdmin").Wait();
}

public partial class Program
{
}