using System.ComponentModel.DataAnnotations;
using IntergratieProject.Domain.project;

namespace IntergratieProject.Domain.ideas;

public class Topic
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Topic must have a theme")]
    public string Theme { get; set; }
    [MaxLength(200)]

    public string Description { get; set; }

    [Required(ErrorMessage = "Topic must belong to a project")]
    public Project Project { get; set; }

    public IEnumerable<Idea> Ideas { get; set; }
}