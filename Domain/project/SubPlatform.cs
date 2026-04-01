using IntergratieProject.Domain.users;

namespace IntergratieProject.Domain.project;

public class SubPlatform
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public Platform Platform { get; set; }
    public Language Language { get; set; }

    public ICollection<SubAdmin> SubAdmins { get; set; } = new List<SubAdmin>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}