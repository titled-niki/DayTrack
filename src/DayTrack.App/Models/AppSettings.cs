namespace DayTrack.Models;

public sealed class AppSettings
{
    public bool FirstRun { get; set; } = true;
    public string Language { get; set; } = "system";
    public string Theme { get; set; } = "system";
    public string WidgetMode { get; set; } = "normal";
    public bool AutoStart { get; set; } = true;
    public bool ShowWidgetOnStartup { get; set; } = true;
    public bool TrackKeyPresses { get; set; } = true;
    public bool TrackMouseClicks { get; set; } = true;
    public bool TrackNetwork { get; set; } = true;
    public bool TrackWindowTitles { get; set; } = false;
    public int AfkSeconds { get; set; } = 300;
    public double? WidgetLeft { get; set; } = null;
    public double? WidgetTop { get; set; } = null;

    // The user selects a parent folder. DayTrack creates <parent>\\DayTrack inside it.
    public string DailyExportParentDirectory { get; set; } =
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

    public bool CreateDesktopShortcut { get; set; } = true;

    public bool LaunchWidgetAfterSetup { get; set; } = true;
}
