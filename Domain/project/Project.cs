using System.Security.Principal;
using IntergratieProject.Domain.ideas;

namespace IntergratieProject.Domain.project;

public class Project
{
    private int Id { get; set; }
    public String Introduction { get; set; }
    public Status Status { get; set; }
    public string Prompt { get; set; }
    public int Duration { get; set; }
    public DateTime ReleaseDate { get; set; }

    public ProjectType Type { get; set; }
     public Media Photo { get; set; }
    public Media Logo { get; set; }

    public IEnumerable<Topic> Topics { get; set; }


    // public SocialMediaPost SocialMediaPost { get; set; }
    // public AiIneraction AiIneraction { get; set; }
    //  public QuestionList  QuestionList { get; set; }
}