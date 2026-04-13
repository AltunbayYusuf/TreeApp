using IntergratieProject.BL;
using IntergratieProject.DAL;
using IntergratieProject.DAL.Ef;
using IntergratieProject.DAL.Identity;
using IntergratieProject.Domain.Ai;
using IntergratieProject.UI.MVC;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vite.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<TreeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TreeDbContext>();

builder.Services.ConfigureApplicationCookie(options => { options.LoginPath = "/Identity/Account/Login"; });

builder.Services.AddHttpClient<IAiService, GeminiService>();
builder.Services.AddScoped<IManager, Manager>();
builder.Services.AddScoped<IIdeaRepository, IdeaRepository>();
builder.Services.AddViteServices();
//150722


// builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//     .AddEntityFrameworkStores<TreeDbContext>()
//     .AddDefaultTokenProviders();


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
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapStaticAssets();

app.MapGet("/", () => Results.Redirect("/kdg-hogeschool/home"));

app.MapControllerRoute(
    name: "subplatform_home",
    pattern: "{subplatform}/home",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "subplatform_root",
    pattern: "{subplatform}",
    defaults: new { controller = "SubPlatform", action = "Index" },
    constraints: new { subplatform = "^(?!Platform|Home|Identity).*$" }
);

app.MapControllerRoute(
    name: "subplatform_project_ideas_create",
    pattern: "{subplatform}/project/{projectId:int}/ideas/create",
    defaults: new { controller = "Idea", action = "Create" });

app.MapControllerRoute(
    name: "subplatform_project_ideas",
    pattern: "{subplatform}/project/{projectId:int}/ideas",
    defaults: new { controller = "Idea", action = "Index" });

app.MapControllerRoute(
    name: "subplatform_project_survey",
    pattern: "{subplatform}/project/{projectId:int}/survey",
    defaults: new { controller = "Survey", action = "Index" });

app.MapControllerRoute(
    name: "subplatform_project",
    pattern: "{subplatform}/project/{id:int}",
    defaults: new { controller = "Project", action = "Index" });

app.MapControllerRoute(
    name: "subplatform_privacy",
    pattern: "{subplatform}/privacy",
    defaults: new { controller = "Home", action = "Privacy" });


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapRazorPages();

app.Run();

void SeedIdentity(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    var adminuser = new ApplicationUser
    {
        UserName = "admin@gmail.com",
        Email = "admin@gmail.com"
    };
    userManager.CreateAsync(adminuser, "Test123!").Wait();

    var kdg = new ApplicationUser
    {
        UserName = "kdg@gmail.com",
        Email = "kdg@gmail.com",
        SubPlatformSlug = "kdg-hogeschool"
    };
    userManager.CreateAsync(kdg, "Test123!").Wait();
    var ap = new ApplicationUser
    {
        UserName = "ap@gmail.com",
        Email = "ap@gmail.com",
        SubPlatformSlug = "ap-hogeschool"
    };
    userManager.CreateAsync(ap, "Test123!").Wait();
    var subAdminRole = new IdentityRole
    {
        Name = CustomIdentityConstants.SubAdminRoleName
    };
    roleManager.CreateAsync(subAdminRole).Wait();
    var generalAdminRole = new IdentityRole
    {
        Name = CustomIdentityConstants.GeneralAdminRoleName
    };
    roleManager.CreateAsync(generalAdminRole).Wait();


    userManager.AddToRoleAsync(adminuser, CustomIdentityConstants.GeneralAdminRoleName).Wait();
    userManager.AddToRoleAsync(kdg, CustomIdentityConstants.SubAdminRoleName).Wait();
    userManager.AddToRoleAsync(ap, CustomIdentityConstants.SubAdminRoleName).Wait();
}

public partial class Program
{
}