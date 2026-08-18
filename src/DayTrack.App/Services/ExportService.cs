using System.Text;
using System.Text.Json;
using DayTrack.Data;

namespace DayTrack.Services;

public sealed class ExportService
{
    private readonly DayTrackDb _db;
    private readonly LocalizationService _loc;
    private readonly WindowsPowerLogReader _power;
    private readonly OutputFolderService _output;

    public ExportService(DayTrackDb db, LocalizationService loc, WindowsPowerLogReader power, OutputFolderService output)
    {
        _db = db;
        _loc = loc;
        _power = power;
        _output = output;
    }

    public string DailyTxt(DateTime day)
    {
        var s = _db.Summary(day, day);
        var apps = _db.Apps(day, day);
        var p = _power.GetDay(day);
        var content = BuildDailyText(day, s, apps, p);
        var filename = DailyFilename(day);

        // Always keep a local safety copy next to the SQLite database.
        var internalDir = Path.Combine(_db.HistoryDirectory, day.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(internalDir);
        CleanupAlternateDailyReports(internalDir, day, filename);
        var internalPath = Path.Combine(internalDir, filename);
        File.WriteAllText(internalPath, content, new UTF8Encoding(true));

        // User-visible copy in the selected location.
        try
        {
            var externalDir = _output.DayDirectory(day);
            Directory.CreateDirectory(externalDir);
            CleanupAlternateDailyReports(externalDir, day, filename);
            var externalPath = Path.Combine(externalDir, filename);
            File.WriteAllText(externalPath, content, new UTF8Encoding(true));
            return externalPath;
        }
        catch
        {
            // If removable/network/user-selected storage is temporarily unavailable,
            // the internal copy still preserves the report.
            return internalPath;
        }
    }

    public string Csv(DateTime f, DateTime t)
    {
        var dir = _output.ExportsDirectory;
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"apps_{f:yyyy-MM-dd}_{t:yyyy-MM-dd}.csv");

        var b = new StringBuilder("App,ActiveSeconds,Launches\r\n");
        foreach (var a in _db.Apps(f, t, 10000))
            b.AppendLine($"\"{a.AppName.Replace("\"", "\"\"")}\",{a.ActiveSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)},{a.Launches}");

        File.WriteAllText(path, b.ToString(), new UTF8Encoding(true));
        return path;
    }

    public string Json(DateTime f, DateTime t)
    {
        var dir = _output.ExportsDirectory;
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"daytrack_{f:yyyy-MM-dd}_{t:yyyy-MM-dd}.json");

        File.WriteAllText(
            path,
            JsonSerializer.Serialize(
                new { From = f, To = t, Summary = _db.Summary(f, t), Apps = _db.Apps(f, t, 10000) },
                new JsonSerializerOptions { WriteIndented = true }));

        return path;
    }

    private string BuildDailyText(DateTime day, DayTrack.Models.RangeSummary s, List<DayTrack.Models.AppUsageRow> apps, PowerDayStats p)
    {
        var b = new StringBuilder();

        b.AppendLine($"{_loc.T("report_title")} — {day:yyyy-MM-dd}");
        b.AppendLine($"{_loc.T("last_updated")}: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        b.AppendLine();

        b.AppendLine(_loc.T("report_general"));
        b.AppendLine($"{_loc.T("pc_on")}: {Dur(s.PcOnSeconds)}");
        b.AppendLine($"{_loc.T("active")}: {Dur(s.ActiveSeconds)}");
        b.AppendLine($"{_loc.T("afk")}: {Dur(s.AfkSeconds)}");
        b.AppendLine($"{_loc.T("locked")}: {Dur(s.LockedSeconds)}");
        b.AppendLine($"{_loc.T("sleep")}: {Dur(s.SleepSeconds)}");
        b.AppendLine($"{_loc.T("paused_time")}: {Dur(s.PausedSeconds)}");
        b.AppendLine();

        b.AppendLine(_loc.T("report_power"));
        b.AppendLine($"{_loc.T("boots")}: {p.Boots}");
        b.AppendLine($"{_loc.T("shutdowns")}: {p.Shutdowns}");
        b.AppendLine($"{_loc.T("unexpected_shutdowns")}: {p.Unexpected}");
        b.AppendLine();

        b.AppendLine(_loc.T("report_input"));
        b.AppendLine($"{_loc.T("key_presses")}: {s.KeyPresses:N0}");
        b.AppendLine($"{_loc.T("mouse_clicks")}: {s.MouseClicks:N0}");
        b.AppendLine(_loc.T("privacy_input_note"));
        b.AppendLine();

        b.AppendLine(_loc.T("report_network"));
        b.AppendLine($"{_loc.T("received")}: {Bytes(s.NetworkReceived)}");
        b.AppendLine($"{_loc.T("sent")}: {Bytes(s.NetworkSent)}");
        b.AppendLine($"{_loc.T("total")}: {Bytes(s.NetworkReceived + s.NetworkSent)}");
        b.AppendLine();

        b.AppendLine(_loc.T("report_apps"));
        int i = 1;
        foreach (var a in apps)
            b.AppendLine($"{i++}. {a.AppName} — {Dur(a.ActiveSeconds)} | {_loc.T("launches")}: {a.Launches}");

        return b.ToString();
    }

    private string DailyFilename(DateTime day)
        => _loc.CurrentLanguage switch
        {
            "ru" => $"Статистика_{day:yyyy-MM-dd}.txt",
            "uk" => $"Статистика_{day:yyyy-MM-dd}.txt",
            "ja" => $"統計_{day:yyyy-MM-dd}.txt",
            "zh" => $"统计_{day:yyyy-MM-dd}.txt",
            _ => $"Statistics_{day:yyyy-MM-dd}.txt"
        };

    private static void CleanupAlternateDailyReports(string dir, DateTime day, string keepFilename)
    {
        string[] names =
        [
            $"Statistics_{day:yyyy-MM-dd}.txt",
            $"Статистика_{day:yyyy-MM-dd}.txt",
            $"統計_{day:yyyy-MM-dd}.txt",
            $"统计_{day:yyyy-MM-dd}.txt",
            "Statistics.txt",
            "Статистика.txt",
            "統計.txt",
            "统计.txt"
        ];

        foreach (var name in names.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (name.Equals(keepFilename, StringComparison.OrdinalIgnoreCase)) continue;
            try
            {
                var path = Path.Combine(dir, name);
                if (File.Exists(path)) File.Delete(path);
            }
            catch { }
        }
    }

    public string Dur(double sec)
    {
        int v = Math.Max(0, (int)sec);
        int h = v / 3600;
        int m = v % 3600 / 60;
        int s = v % 60;

        return h > 0
            ? $"{h} {_loc.T("unit_h")} {m} {_loc.T("unit_min")} {s} {_loc.T("unit_sec")}"
            : $"{m} {_loc.T("unit_min")} {s} {_loc.T("unit_sec")}";
    }

    public static string Bytes(long b)
    {
        double v = Math.Max(0, b);
        string[] u = ["B", "KB", "MB", "GB", "TB"];
        int i = 0;

        while (v >= 1024 && i < u.Length - 1)
        {
            v /= 1024;
            i++;
        }

        return $"{v:0.##} {u[i]}";
    }
}
