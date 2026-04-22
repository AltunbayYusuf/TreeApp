using System.ComponentModel.DataAnnotations;

namespace IntegratieProject.BL.Domain.ideas;

public class Media
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Image must have a uri")]
    [MaxLength(200)]
    public string Uri { get; set; }
}