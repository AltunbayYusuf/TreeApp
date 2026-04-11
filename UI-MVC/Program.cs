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

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapGet("/", () => Results.Redirect("/kdg-hogeschool"));



app.MapControllerRoute(
    name: "subplatform_root",
    pattern: "{subplatform}",
    defaults: new { controller = "Project", action = "RedirectToFirstProject" });

app.MapControllerRoute(
    name: "subplatform_short",
    pattern: "{subplatform}/{controller=Project}/{id:int}",
    defaults: new { action = "Index" });

app.MapControllerRoute(
    name: "subplatform_default",
    pattern: "{subplatform}/{controller=Project}",
    defaults: new { action = "Index", id = 1 });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "subplatform_action",
    pattern: "{subplatform}/{controller=Project}/{action=Index}/{id?}");


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
        Email = "kdg@gmail.com"
    };
    userManager.CreateAsync(kdg, "Test123!").Wait();

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
}

public partial class Program
{
}