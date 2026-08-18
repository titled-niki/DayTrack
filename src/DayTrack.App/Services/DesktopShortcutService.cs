using System.Runtime.InteropServices;

namespace DayTrack.Services;

public sealed class DesktopShortcutService
{
    public string ShortcutPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
        "DayTrack.lnk");

    public void SetEnabled(bool enabled)
    {
        if (enabled)
            CreateOrUpdate();
        else
            Remove();
    }

    private void CreateOrUpdate()
    {
        object? shell = null;
        object? shortcut = null;

        try
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType is null)
            {
                RuntimeLog.Write("Desktop shortcut: WScript.Shell unavailable.");
                return;
            }

            shell = Activator.CreateInstance(shellType);
            if (shell is null)
            {
                RuntimeLog.Write("Desktop shortcut: failed to create WScript.Shell.");
                return;
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

            RuntimeLog.Write(
                $"Desktop shortcut: {(File.Exists(ShortcutPath) ? "OK" : "FAILED")} | {ShortcutPath} -> {exe}");
        }
        catch (Exception ex)
        {
            RuntimeLog.WriteException("Desktop shortcut error", ex);
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

    private void Remove()
    {
        try
        {
            if (File.Exists(ShortcutPath))
                File.Delete(ShortcutPath);

            RuntimeLog.Write($"Desktop shortcut removed: {ShortcutPath}");
        }
        catch (Exception ex)
        {
            RuntimeLog.WriteException("Desktop shortcut remove error", ex);
        }
    }
}
