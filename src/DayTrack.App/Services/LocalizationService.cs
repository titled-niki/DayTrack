using System.Globalization;
using System.Text.Json;

namespace DayTrack.Services;

public sealed class LocalizationService
{
    private Dictionary<string, string> _strings = new(StringComparer.OrdinalIgnoreCase);

    public string CurrentLanguage { get; private set; } = "en";
    public event Action? LanguageChanged;

    public void Apply(string setting)
    {
        var lang = setting.Equals("system", StringComparison.OrdinalIgnoreCase)
            ? DetectSystemLanguage()
            : Normalize(setting);

        CurrentLanguage = lang;

        var path = Path.Combine(AppContext.BaseDirectory, "Localization", $"{lang}.json");
        try
        {
            _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))
                       ?? new Dictionary<string, string>();
        }
        catch
        {
            // Safe fallback to English if a translation file is unavailable.
            CurrentLanguage = "en";
            var en = Path.Combine(AppContext.BaseDirectory, "Localization", "en.json");
            try
            {
                _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(en))
                           ?? new Dictionary<string, string>();
            }
            catch
            {
                _strings = new Dictionary<string, string>();
            }
        }

        LanguageChanged?.Invoke();
    }

    public string T(string key)
        => _strings.TryGetValue(key, out var value) ? value : key;

    private static string DetectSystemLanguage()
    {
        var name = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();
        var two = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

        if (two == "ru") return "ru";
        if (two == "uk") return "uk";
        if (two == "ja") return "ja";
        if (two == "zh" || name.StartsWith("zh-")) return "zh";
        return "en";
    }

    private static string Normalize(string value)
    {
        value = value.ToLowerInvariant();
        return value switch
        {
            "ru" => "ru",
            "uk" => "uk",
            "ja" => "ja",
            "zh" => "zh",
            _ => "en"
        };
    }
}
