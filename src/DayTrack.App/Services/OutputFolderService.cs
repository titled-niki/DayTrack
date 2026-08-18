using DayTrack.Models;

namespace DayTrack.Services;

public sealed class OutputFolderService
{
    private readonly SettingsService _settings;

    public OutputFolderService(SettingsService settings)
    {
        _settings = settings;
    }

    public string ParentDirectory
    {
        get
        {
            var configured = _settings.Current.DailyExportParentDirectory;
            if (!string.IsNullOrWhiteSpace(configured))
                return configured;

            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }
    }

    public string RootDirectory => Path.Combine(ParentDirectory, "DayTrack");

    public string DayDirectory(DateTime day)
        => Path.Combine(RootDirectory, day.ToString("yyyy-MM-dd"));

    public string ExportsDirectory => Path.Combine(RootDirectory, "Exports");

    public void EnsureRoot()
    {
        Directory.CreateDirectory(RootDirectory);
    }

    public static string NormalizeSelectedParent(string selected)
    {
        if (string.IsNullOrWhiteSpace(selected))
            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        selected = Path.GetFullPath(selected.Trim());
        var leaf = new DirectoryInfo(selected).Name;

        // If the user selected an existing DayTrack folder, treat its parent as the base
        // to avoid creating DayTrack\\DayTrack.
        if (leaf.Equals("DayTrack", StringComparison.OrdinalIgnoreCase))
        {
            var parent = Directory.GetParent(selected);
            if (parent is not null)
                return parent.FullName;
        }

        return selected;
    }
}
