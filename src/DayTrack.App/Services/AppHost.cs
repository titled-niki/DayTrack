using DayTrack.Data;
using DayTrack.Windows;

namespace DayTrack.Services;

public sealed class AppHost : IDisposable
{
    public SettingsService Settings { get; } = new();
    public LocalizationService Loc { get; } = new();
    public ThemeService Theme { get; } = new();
    public AutoStartService AutoStart { get; } = new();
    public DesktopShortcutService DesktopShortcut { get; } = new();
    public StartMenuShortcutService StartMenuShortcut { get; } = new();
    public DayTrackDb Db { get; } = new();
    public WindowsPowerLogReader Power { get; } = new();
    public OutputFolderService OutputFolders { get; }
    public ExportService Export { get; }
    public CollectorService Collector { get; }

    public WidgetWindow? Widget { get; private set; }
    public MainWindow? Dashboard { get; private set; }
    public SettingsWindow? SettingsWindow { get; private set; }

    private System.Windows.Forms.NotifyIcon? _tray;

    public AppHost()
    {
        OutputFolders = new OutputFolderService(Settings);
        Export = new ExportService(Db, Loc, Power, OutputFolders);
        Collector = new CollectorService(Db, Settings, Export);
    }

    public void Start(string[] args)
    {
        RuntimeLog.Write(
            $"AppHost.Start | savedLanguage={Settings.Current.Language} | firstRun={Settings.Current.FirstRun}");

        Loc.Apply(Settings.Current.Language);
        Theme.Apply(Settings.Current.Theme);

        bool wasFirstRun = Settings.Current.FirstRun;
        bool launchWidgetAfterSetup = Settings.Current.LaunchWidgetAfterSetup;

        if (wasFirstRun)
        {
            var onboarding = new OnboardingWindow(this);
            var result = onboarding.ShowDialog();

            if (result != true)
            {
                RuntimeLog.Write("Onboarding cancelled.");
                System.Windows.Application.Current.Shutdown();
                return;
            }

            launchWidgetAfterSetup = onboarding.LaunchWidgetAfterFinish;
        }

        CreateTray();
        Collector.Start();

        AutoStart.SetEnabled(Settings.Current.AutoStart);
        DesktopShortcut.SetEnabled(Settings.Current.CreateDesktopShortcut);
        StartMenuShortcut.Ensure();

        try { OutputFolders.EnsureRoot(); }
        catch (Exception ex) { RuntimeLog.WriteException("Ensure output root", ex); }

        try
        {
            var report = Export.DailyTxt(DateTime.Today);
            RuntimeLog.Write(
                $"Report after startup | setting={Settings.Current.Language} | resolved={Loc.CurrentLanguage} | path={report}");
        }
        catch (Exception ex)
        {
            RuntimeLog.WriteException("Startup report", ex);
        }

        bool startup = args.Any(a =>
            a.Equals("--startup", StringComparison.OrdinalIgnoreCase));

        if (wasFirstRun)
        {
            if (launchWidgetAfterSetup)
                ShowWidget();
        }
        else if (!startup || Settings.Current.ShowWidgetOnStartup)
        {
            ShowWidget();
        }

        RuntimeLog.Write("AppHost.Start completed.");
    }

    public void ShowWidget()
    {
        if (Widget is null)
        {
            Widget = new WidgetWindow(this);
            Widget.Closed += (_, _) => Widget = null;
        }

        if (!Widget.IsVisible) Widget.Show();
        Widget.Activate();
    }

    public void ShowDashboard()
    {
        if (Dashboard is null)
        {
            Dashboard = new MainWindow(this);
            Dashboard.Closed += (_, _) => Dashboard = null;
        }

        if (!Dashboard.IsVisible) Dashboard.Show();
        Dashboard.Activate();
    }

    public void ShowSettings()
    {
        if (SettingsWindow is null)
        {
            SettingsWindow = new SettingsWindow(this);
            SettingsWindow.Closed += (_, _) => SettingsWindow = null;
        }

        if (!SettingsWindow.IsVisible) SettingsWindow.Show();
        SettingsWindow.Activate();
    }

    public void ApplySettings()
    {
        Settings.Save();

        Loc.Apply(Settings.Current.Language);
        Theme.Apply(Settings.Current.Theme);

        RuntimeLog.Write(
            $"ApplySettings | setting={Settings.Current.Language} | resolved={Loc.CurrentLanguage}");

        AutoStart.SetEnabled(Settings.Current.AutoStart);
        DesktopShortcut.SetEnabled(Settings.Current.CreateDesktopShortcut);
        StartMenuShortcut.Ensure();

        try { OutputFolders.EnsureRoot(); }
        catch (Exception ex) { RuntimeLog.WriteException("ApplySettings output root", ex); }

        Widget?.ApplySettingsAndLocalization();
        Dashboard?.ApplyLocalization();
        BuildMenu();

        try
        {
            var report = Export.DailyTxt(DateTime.Today);
            RuntimeLog.Write($"ApplySettings report: {report}");
        }
        catch (Exception ex)
        {
            RuntimeLog.WriteException("ApplySettings report", ex);
        }
    }

    public void OpenHistory()
    {
        string path;
        try
        {
            OutputFolders.EnsureRoot();
            path = OutputFolders.RootDirectory;
        }
        catch
        {
            Directory.CreateDirectory(Db.HistoryDirectory);
            path = Db.HistoryDirectory;
        }

        System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo("explorer.exe", $"\"{path}\"")
            { UseShellExecute = true });
    }

    private void CreateTray()
    {
        System.Drawing.Icon icon;
        try
        {
            icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!)
                   ?? System.Drawing.SystemIcons.Application;
        }
        catch
        {
            icon = System.Drawing.SystemIcons.Application;
        }

        _tray = new System.Windows.Forms.NotifyIcon
        {
            Visible = true,
            Text = "DayTrack",
            Icon = icon
        };

        _tray.DoubleClick += (_, _) => ShowWidget();
        BuildMenu();
    }

    private void BuildMenu()
    {
        if (_tray is null) return;

        var old = _tray.ContextMenuStrip;
        var menu = new System.Windows.Forms.ContextMenuStrip();

        menu.Items.Add(Loc.T("tray_open_widget"), null, (_, _) => ShowWidget());
        menu.Items.Add(Loc.T("tray_open_dashboard"), null, (_, _) => ShowDashboard());
        menu.Items.Add(Loc.T("tray_open_history"), null, (_, _) => OpenHistory());
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        var pause = new System.Windows.Forms.ToolStripMenuItem(Loc.T("pause_tracking"));
        pause.DropDownItems.Add(Loc.T("pause_5"), null, (_, _) => Collector.PauseFor(TimeSpan.FromMinutes(5)));
        pause.DropDownItems.Add(Loc.T("pause_15"), null, (_, _) => Collector.PauseFor(TimeSpan.FromMinutes(15)));
        pause.DropDownItems.Add(Loc.T("pause_30"), null, (_, _) => Collector.PauseFor(TimeSpan.FromMinutes(30)));
        pause.DropDownItems.Add(Loc.T("pause_60"), null, (_, _) => Collector.PauseFor(TimeSpan.FromHours(1)));
        pause.DropDownItems.Add(Loc.T("pause_until_resume"), null, (_, _) => Collector.PauseFor(null));
        menu.Items.Add(pause);

        menu.Items.Add(Loc.T("resume_tracking"), null, (_, _) => Collector.Resume());
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add(Loc.T("settings"), null, (_, _) => ShowSettings());
        menu.Items.Add(Loc.T("exit"), null, (_, _) => Exit());

        _tray.ContextMenuStrip = menu;
        old?.Dispose();
    }

    public void Exit()
    {
        Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Collector.Dispose();
        try { Db.Backup(); } catch { }

        if (_tray is not null)
        {
            _tray.Visible = false;
            _tray.ContextMenuStrip?.Dispose();
            _tray.Dispose();
            _tray = null;
        }
    }
}
