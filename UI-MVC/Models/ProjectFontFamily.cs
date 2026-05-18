namespace IntegratieProject.UI.MVC.Models;

public static class ProjectFontFamily
{
    public static string ToCssStack(string fontFamily)
    {
        return fontFamily switch
        {
            "Arial" => "Arial, Helvetica, sans-serif",
            "Georgia" => "Georgia, 'Times New Roman', serif",
            "Verdana" => "Verdana, Geneva, sans-serif",
            "Trebuchet MS" => "'Trebuchet MS', Arial, sans-serif",
            _ => "Inter, system-ui, -apple-system, 'Segoe UI', sans-serif"
        };
    }
}
