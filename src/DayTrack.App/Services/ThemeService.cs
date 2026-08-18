using Microsoft.Win32;

namespace DayTrack.Services;

public sealed class ThemeService
{
    public void Apply(string setting)
    {
        var theme = setting == "system" ? (IsLight() ? "light" : "dark") : setting;
        bool light = theme == "light";

        Set("BgBrush", light ? "#F3F5F8" : "#101319");
        Set("PanelBrush", light ? "#FFFFFF" : "#171C24");
        Set("Panel2Brush", light ? "#E9EDF3" : "#222834");
        Set("TextBrush", light ? "#15181D" : "#F3F4F6");
        Set("MutedTextBrush", light ? "#4E5968" : "#B5BEC9");
        Set("BorderBrush", light ? "#C7CFDB" : "#3C4657");
        Set("AccentBrush", light ? "#DCE5F3" : "#2A3342");
        Set("DangerBrush", "#E36B6B");
    }

    private static void Set(string key, string color)
        => System.Windows.Application.Current.Resources[key] =
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));

    private static bool IsLight()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int i && i > 0;
        }
        catch
        {
            return false;
        }
    }
}
