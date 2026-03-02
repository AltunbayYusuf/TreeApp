using IntergratieProject.Domain.users;

namespace IntergratieProject.Domain.project;

public class Platform
{
    public String CompanyName { get; set; }
    public GeneralAdmin GeneralAdmin { get; set; }

    public SubPlatform SubPlatform { get; set; }
}