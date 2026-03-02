using IntergratieProject.Domain.users;

namespace IntergratieProject.Domain.project;

public class SubPlatform
{
    public string CompanyName { get; set; }
    public Platform Platform { get; set; }
    public Language Language { get; set; }
    public IEnumerable<SubAdmin> SubAdmins { get; set; }
    public IEnumerable<Project> Projects { get; set; }
}