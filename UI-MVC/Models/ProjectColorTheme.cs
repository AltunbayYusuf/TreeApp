namespace IntegratieProject.UI.MVC.Models;

public static class ProjectColorTheme
{
    public static string ToCssVariables(string theme)
    {
        return theme switch
        {
            "Purple" => "--echo-primary:#7c3aed;--echo-primary-dark:#5b21b6;--survey-accent:#7c3aed;--survey-accent-rgb:124,58,237;--chat-primary:#7c3aed;--chat-primary-dk:#5b21b6;--chat-primary-rgb:124,58,237;",
            "Green" => "--echo-primary:#16a34a;--echo-primary-dark:#166534;--survey-accent:#16a34a;--survey-accent-rgb:22,163,74;--chat-primary:#16a34a;--chat-primary-dk:#166534;--chat-primary-rgb:22,163,74;",
            "Orange" => "--echo-primary:#f97316;--echo-primary-dark:#c2410c;--survey-accent:#f97316;--survey-accent-rgb:249,115,22;--chat-primary:#f97316;--chat-primary-dk:#c2410c;--chat-primary-rgb:249,115,22;",
            _ => "--echo-primary:#2563eb;--echo-primary-dark:#1d4ed8;--survey-accent:#2563eb;--survey-accent-rgb:37,99,235;--chat-primary:#2563eb;--chat-primary-dk:#1d4ed8;--chat-primary-rgb:37,99,235;"
        };
    }

    public static string GetPrimaryColor(string theme)
    {
        return theme switch
        {
            "Purple" => "#7c3aed",
            "Green" => "#16a34a",
            "Orange" => "#f97316",
            _ => "#2563eb"
        };
    }

    public static string GetDarkColor(string theme)
    {
        return theme switch
        {
            "Purple" => "#5b21b6",
            "Green" => "#166534",
            "Orange" => "#c2410c",
            _ => "#1d4ed8"
        };
    }
}