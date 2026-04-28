using System.ComponentModel.DataAnnotations;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.Questions;

namespace IntegratieProject.BL.Domain.questions;

public class Question
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Question must have a description")]
    [MaxLength(200)]
    public string Description { get; set; }

    [Required(ErrorMessage = "Question must be in a section")]
    public Section Section { get; set; }

    [Required(ErrorMessage = "Question must have a questionType")]
    public QuestionType QuestionType { get; set; }

    public Media Image { get; set; }
    public List<Answer> Answers { get; set; }

    public List<QuestionOption> Options { get; set; } = new();
    
    public ICollection<ConditionalQuestion> ConditionalQuestions { get; set; } = new List<ConditionalQuestion>();
    public bool IsRequired { get; set; } = true;

    // optioneel voor Range-vragen
    public int? RangeMin { get; set; }
    public int? RangeMax { get; set; }
    [MaxLength(200)] public string RangeMinLabel { get; set; }
    [MaxLength(200)] public string RangeMaxLabel { get; set; }
}