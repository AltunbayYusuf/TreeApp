using System.Diagnostics;
using IntergratieProject.DAL.Identity;
using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.Domain.Questions;
using IntergratieProject.Domain.users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntergratieProject.DAL.Ef;

public class TreeDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<GeneralAdmin> GeneralAdmins { get; set; }
    public DbSet<SubAdmin> SubAdmins { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Idea> Ideas { get; set; }
    public DbSet<Reaction> Reactions { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Platform> Platforms { get; set; }
    public DbSet<SubPlatform> SubPlatforms { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<SurveyResponse> SurveyResponses { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionList> QuestionList { get; set; }
    public DbSet<Section> Section { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }

    public TreeDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GeneralAdmin>()
            .HasMany(g => g.SubAdmins)
            .WithOne(s => s.GeneralAdmin);

        modelBuilder.Entity<SubAdmin>()
            .HasOne(s => s.SubPlatform)
            .WithMany(sp => sp.SubAdmins);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Ideas)
            .WithOne(i => i.User)
            .HasForeignKey(i => i.UserId);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Reactions)
            .WithOne(r => r.User);

        modelBuilder.Entity<User>()
            .HasMany(u => u.SurveyResponses)
            .WithOne(sr => sr.User)
            .HasForeignKey(sr => sr.UserId);

        modelBuilder.Entity<Reaction>()
            .HasOne(r => r.Idea)
            .WithMany(i => i.Reactions);

        modelBuilder.Entity<Topic>()
            .HasMany(t => t.Ideas)
            .WithOne(i => i.Topic);

        modelBuilder.Entity<Platform>()
            .HasOne(p => p.GeneralAdmin)
            .WithOne(g => g.Platform)
            .HasForeignKey<GeneralAdmin>("PlatformId");

        modelBuilder.Entity<SubPlatform>()
            .HasOne(sp => sp.Platform)
            .WithMany(p => p.SubPlatforms);

        modelBuilder.Entity<SubPlatform>()
            .HasMany(sp => sp.Projects)
            .WithOne(p => p.SubPlatform)
            .HasForeignKey(p => p.SubPlatformId);
        
        modelBuilder.Entity<SubPlatform>()
            .HasIndex(sp => sp.Slug)
            .IsUnique();

        modelBuilder.Entity<Project>()
            .HasOne(p => p.QuestionList)
            .WithOne(ql => ql.Project)
            .HasForeignKey<QuestionList>(ql => ql.ProjectId);

        modelBuilder.Entity<Project>()
            .HasMany(p => p.Topics)
            .WithOne(t => t.Project);

        modelBuilder.Entity<Project>()
            .HasMany(p => p.SurveyResponses)
            .WithOne(sr => sr.Project)
            .HasForeignKey(sr => sr.ProjectId);

        modelBuilder.Entity<QuestionList>()
            .HasMany(ql => ql.Sections)
            .WithOne(s => s.QuestionList)
            .HasForeignKey("QuestionListId");

        modelBuilder.Entity<Section>()
            .HasMany(s => s.Questions)
            .WithOne(q => q.Section)
            .HasForeignKey("SectionId");

        modelBuilder.Entity<Question>()
            .HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId);

        modelBuilder.Entity<SurveyResponse>()
            .HasMany(sr => sr.Answers)
            .WithOne(a => a.SurveyResponse)
            .HasForeignKey(a => a.SurveyResponseId);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Data source=treeAppDb.db");
        }

        optionsBuilder.LogTo(message => Debug.WriteLine(message), LogLevel.Information);
    }

    public bool CreateDatabase(bool dropDatabase)
    {
        if (dropDatabase)
        {
            Database.EnsureDeleted();
        }

        return Database.EnsureCreated();
    }
}