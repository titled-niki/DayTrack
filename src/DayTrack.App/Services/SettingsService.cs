using System.Text.Json;
using DayTrack.Models;

namespace DayTrack.Services;

public sealed class SettingsService
{
    private readonly string _dir;
    private readonly string _path;

    public AppSettings Current { get; private set; } = new();

    public SettingsService()
    {
        _dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DayTrack");

        _path = Path.Combine(_dir, "settings.json");

        Directory.CreateDirectory(_dir);
        Load();
    }

    public void Load()
    {
        try
        {
            if (!File.Exists(_path))
            {
                Current = new AppSettings();
                return;
            }

            var json = File.ReadAllText(_path);
            Current = JsonSerializer.Deserialize<AppSettings>(json)
                      ?? new AppSettings();

            Sanitize();
        }
        catch (Exception ex)
        {
            // If an old/broken settings file exists, do not block DayTrack.
            RuntimeLog.WriteException("Settings load failed; using defaults", ex);
            Current = new AppSettings();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(_dir);
        Sanitize();

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(Current, options);

        // Atomic-ish save: serialize first, then write temp, then replace.
        // This prevents a failed serialization/write from corrupting settings.json.
        var tempPath = _path + ".tmp";

        File.WriteAllText(tempPath, json);

        if (File.Exists(_path))
        {
            try
            {
                File.Replace(tempPath, _path, null);
            }
            catch
            {
                File.Copy(tempPath, _path, true);
                File.Delete(tempPath);
            }
        }
        else
        {
            File.Move(tempPath, _path);
        }

        RuntimeLog.Write(
            $"Settings saved | language={Current.Language} | theme={Current.Theme} | " +
            $"widgetLeft={Current.WidgetLeft?.ToString() ?? "null"} | " +
            $"widgetTop={Current.WidgetTop?.ToString() ?? "null"}");
    }

    private void Sanitize()
    {
        if (Current.WidgetLeft is double left && !double.IsFinite(left))
            Current.WidgetLeft = null;

        if (Current.WidgetTop is double top && !double.IsFinite(top))
            Current.WidgetTop = null;

        if (string.IsNullOrWhiteSpace(Current.Language))
            Current.Language = "system";

        if (string.IsNullOrWhiteSpace(Current.Theme))
            Current.Theme = "system";

        if (string.IsNullOrWhiteSpace(Current.WidgetMode))
            Current.WidgetMode = "normal";
    }
}
