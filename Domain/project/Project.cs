using IntergratieProject.Domain.Ai;
using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.Questions;
using IntergratieProject.Domain.socialeMedia;

namespace IntergratieProject.Domain.project;

public class Project
{
    private int Id { get; set; }
    public string Introduction { get; set; }
    public Status Status { get; set; }
    public string Prompt { get; set; }
    public int Duration { get; set; }
    public DateTime ReleaseDate { get; set; }

    public ProjectType Type { get; set; }
    public Media Photo { get; set; }
    public Media Logo { get; set; }
    public SubPlatform SubPlatform { get; set; }
    
    public IEnumerable<Topic> Topics { get; set; }
    

    public SocialMediaPost SocialMediaPost { get; set; }
    public AiIntegration AiIntegration { get; set; }
    public QuestionList  QuestionList { get; set; }
}