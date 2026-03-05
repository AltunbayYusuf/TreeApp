namespace IntergratieProject.BL;

public interface  IManager
{
    public Task<string> AskAiForIdea(string idea);
}