using IntergratieProject.Domain.users;

namespace IntergratieProject.Domain.project;

public class Platform
{
    public int Id { get; set; }

    public string CompanyName { get; set; }
    public GeneralAdmin GeneralAdmin { get; set; }

    public IEnumerable<SubPlatform> SubPlatforms { get; set; }
}