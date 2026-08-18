using System.Runtime.InteropServices;

namespace DayTrack.Services;

public sealed class StartMenuShortcutService
{
    public string ShortcutPath
    {
        get
        {
            var programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);

            if (string.IsNullOrWhiteSpace(programs))
            {
                programs = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Microsoft",
                    "Windows",
                    "Start Menu",
                    "Programs");
            }

            return Path.Combine(programs, "DayTrack.lnk");
        }
    }

    public bool Ensure()
    {
        object? shell = null;
        object? shortcut = null;

        try
        {
            var directory = Path.GetDirectoryName(ShortcutPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType is null)
            {
                RuntimeLog.Write("Start Menu shortcut: WScript.Shell unavailable.");
                return false;
            }

            shell = Activator.CreateInstance(shellType);
            if (shell is null)
            {
                RuntimeLog.Write("Start Menu shortcut: failed to create WScript.Shell.");
                return false;
            }

            dynamic dynamicShell = shell;
            shortcut = dynamicShell.CreateShortcut(ShortcutPath);
            dynamic dynamicShortcut = shortcut;

            var exe = Environment.ProcessPath
                      ?? Path.Combine(AppContext.BaseDirectory, "DayTrack.exe");

            dynamicShortcut.TargetPath = exe;
            dynamicShortcut.WorkingDirectory = AppContext.BaseDirectory;
            dynamicShortcut.Description = "DayTrack";
            dynamicShortcut.IconLocation = exe + ",0";
            dynamicShortcut.Save();

            var ok = File.Exists(ShortcutPath);
            RuntimeLog.Write(
                $"Start Menu shortcut: {(ok ? "OK" : "FAILED")} | {ShortcutPath} -> {exe}");

            return ok;
        }
        catch (Exception ex)
        {
            RuntimeLog.WriteException("Start Menu shortcut error", ex);
            return false;
        }
        finally
        {
            try
            {
                if (shortcut is not null && Marshal.IsComObject(shortcut))
                    Marshal.FinalReleaseComObject(shortcut);
            }
            catch { }

            try
            {
                if (shell is not null && Marshal.IsComObject(shell))
                    Marshal.FinalReleaseComObject(shell);
            }
            catch { }
        }
    }
}
