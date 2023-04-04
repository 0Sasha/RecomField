namespace RecomField.Models;

public class UserSettings
{
    public int Id { get; set; }
    public int MinuteTimeOffset { get; set; }
    public bool DarkTheme { get; set; }
    public Language InterfaceLanguage { get; set; } = Language.English;
    
    public enum Language
    {
        English,
        Russian
    }
}
