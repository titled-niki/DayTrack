namespace DayTrack.Services;

public static class RuntimeLog
{
    private static string DirectoryPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DayTrack");

    public static string FilePath => Path.Combine(DirectoryPath, "startup.log");

    public static void Write(string message)
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            File.AppendAllText(
                FilePath,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
        }
        catch { }
    }

    public static void WriteException(string context, Exception ex)
        => Write($"{context}: {ex}");
}
