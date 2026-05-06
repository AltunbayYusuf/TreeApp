using Google.Cloud.AIPlatform.V1;
using Google.Cloud.VertexAI.Extensions;
using IntegratieProject.BL;
using IntegratieProject.BL.Ai;
using IntegratieProject.BL.interfaces;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.interfaces;
using IntegratieProject.DAL.Ef;
using IntegratieProject.DAL.Identity;
using IntegratieProject.DAL.Interfaces;
using IntegratieProject.UI.MVC;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
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

builder.Services.AddSession();

// Cookie policy: werkt ook over HTTP (GCP deploy zonder HTTPS)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Antiforgery cookie policy: zelfde reden
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Ai Toegevoegd: tijdelijke fix -> custom DataProtection verwijderd.
// Reden: keys op /root/.aspnet/... zijn in cloud/container vaak niet echt persistent
// en kunnen 400 errors geven bij login/antiforgery validatie.
// builder.Services.AddDataProtection()
//     .PersistKeysToFileSystem(new DirectoryInfo("/root/.aspnet/DataProtection-Keys"))
//     .SetApplicationName("IntergratieProject");
var predictionBuilder = new PredictionServiceClientBuilder
{
    Endpoint = builder.Configuration["Google:Endpoint"]
               ?? "us-central1-aiplatform.googleapis.com"
};

var projectId = builder.Configuration["Google:ProjectId"]
                ?? throw new InvalidOperationException("Missing configuration: Google:ProjectId");

var location = builder.Configuration["Google:Location"]
               ?? throw new InvalidOperationException("Missing configuration: Google:Location");

var moderationModel = builder.Configuration["Google:ModerationModel"]
                      ?? "gemini-2.5-flash-lite";

var chatModelResource = EndpointName.FormatProjectLocationPublisherModel(
    projectId,
    location,
    "google",
    moderationModel);
var chatClient = await predictionBuilder.BuildIChatClientAsync(chatModelResource);


builder.Services.AddChatClient(chatClient);
builder.Services.AddScoped<IAiProvider, VertexAiProvider>();
builder.Services.AddScoped<IAiPromptService, AiPromptService>();
builder.Services.AddScoped<IAiModerationService, AiModerationService>();
builder.Services.AddScoped<IAiSurveyGenerationService, AiSurveyGenerationService>();
builder.Services.AddScoped<IAiUsageManager, AiUsageManager>();
builder.Services.AddScoped<IAiSummaryIdeas, AiSummaryIdeas>();

builder.Services.AddScoped<IAiRepository, AiRepository>();
builder.Services.AddScoped<IAiUsageRepository, AiUsageRepository>();

builder.Services.AddScoped<IIntroTextService, IntroTextService>();
builder.Services.AddScoped<IImageGenerationService, DummyImageGenerationService>();
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IIdeaRepository, IdeaRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IReactionRepository, ReactionRepository>();
builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISubplatformRepository, SubplatformRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();

builder.Services.AddScoped<IManager, Manager>();
builder.Services.AddScoped<IIdeaManager, IdeaManager>();
builder.Services.AddScoped<IProjectManager, ProjectManager>();
builder.Services.AddScoped<IQuestionManager, QuestionManager>();
builder.Services.AddScoped<IReactionManager, ReactionManager>();
builder.Services.AddScoped<ISurveyManager, SurveyManager>();
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<ISubplatformManager, SubplatformManager>();
builder.Services.AddScoped<ITopicManager, TopicManager>();
builder.Services.AddScoped<IProjectStatisticsManager, StatisticsManager>();
builder.Services.AddViteServices();
//150722


// builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//     .AddEntityFrameworkStores<TreeDbContext>()
//     .AddDefaultTokenProviders();


var app = builder.Build();

// Ai Toegevoegd: Forwarded headers zo vroeg mogelijk zetten.
// Dit helpt wanneer je app achter een proxy/load balancer draait.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Environment.IsDevelopment())
{
    app.UseViteDevelopmentServer();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<TreeDbContext>();
    if (context.CreateDatabase(dropDatabase: true))
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        SeedIdentity(userManager, roleManager);
        DataSeeder.Seed(context);
    }
}

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// HTTPS redirect alleen lokaal. Op GCP draaien we HTTP op poort 8080.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

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
//  kdg-hogeschool/Project -> standaard een /1 achter de schermen 
app.MapControllerRoute(
    name: "subplatform_default",
    pattern: "{subplatform}/{controller=Project}",
    defaults: new { action = "Index", id = 1 });

app.MapControllerRoute(
    name: "subplatform_action",
    pattern: "{subplatform}/{controller=Project}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Project}/{action=Index}/{id=1}");


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