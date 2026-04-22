using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.BL.Domain.users;

public interface IAdmin
{
    [Required]
    public string Name { get; set; }
}