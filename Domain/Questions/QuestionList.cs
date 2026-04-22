using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.project;

namespace IntegratieProject.BL.Domain.questions;

public class QuestionList
{
    public int Id { get; set; }

    [Required(ErrorMessage = "QuestionList must belong to a projectID")]
    public int ProjectId { get; set; }

    public Project Project { get; set; }

    public List<Section> Sections { get; set; } = new();
}