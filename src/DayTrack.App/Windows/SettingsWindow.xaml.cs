using DayTrack.Services;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DayTrack.Windows;

public partial class SettingsWindow : Window
{
    private readonly AppHost _h;
    private readonly string _originalLanguage;
    private readonly string _originalTheme;
    private string _selectedExportParent;
    private bool _saved;

    public SettingsWindow(AppHost h)
    {
        InitializeComponent();
        _h = h;
        _originalLanguage = h.Settings.Current.Language;
        _originalTheme = h.Settings.Current.Theme;
        _selectedExportParent = OutputFolderService.NormalizeSelectedParent(
            h.Settings.Current.DailyExportParentDirectory);

        LoadValues();
        ApplyLocalization();
        UpdateExportFolderDisplay();

        LanguageCombo.SelectionChanged += LanguageCombo_SelectionChanged;
        ThemeCombo.SelectionChanged += ThemeCombo_SelectionChanged;
        Closing += SettingsWindow_Closing;
    }

    private void LoadValues()
    {
        SelectByTag(LanguageCombo, _h.Settings.Current.Language);
        SelectByTag(ThemeCombo, _h.Settings.Current.Theme);
        SelectByTag(WidgetModeCombo, _h.Settings.Current.WidgetMode);
        AfkTextBox.Text = _h.Settings.Current.AfkSeconds.ToString();
        AutoStartCheck.IsChecked = _h.Settings.Current.AutoStart;
        ShowWidgetCheck.IsChecked = _h.Settings.Current.ShowWidgetOnStartup;
        DesktopShortcutCheck.IsChecked = _h.Settings.Current.CreateDesktopShortcut;
        KeysCheck.IsChecked = _h.Settings.Current.TrackKeyPresses;
        ClicksCheck.IsChecked = _h.Settings.Current.TrackMouseClicks;
        NetworkCheck.IsChecked = _h.Settings.Current.TrackNetwork;
        WindowTitlesCheck.IsChecked = _h.Settings.Current.TrackWindowTitles;
    }

    public void ApplyLocalization()
    {
        HeaderText.Text = Title = _h.Loc.T("settings");
        LanguageLabel.Text = _h.Loc.T("language");
        ThemeLabel.Text = _h.Loc.T("theme");
        WidgetModeLabel.Text = _h.Loc.T("widget_mode");
        ExportFolderLabel.Text = _h.Loc.T("daily_reports_folder");
        BrowseFolderButton.Content = _h.Loc.T("browse");
        ExportFolderHint.Text = _h.Loc.T("export_folder_hint");
        AfkLabel.Text = _h.Loc.T("afk_threshold");
        AutoStartCheck.Content = _h.Loc.T("autostart");
        ShowWidgetCheck.Content = _h.Loc.T("show_widget_startup");
        DesktopShortcutCheck.Content = _h.Loc.T("create_desktop_shortcut");
        KeysCheck.Content = _h.Loc.T("count_keys");
        ClicksCheck.Content = _h.Loc.T("count_clicks");
        NetworkCheck.Content = _h.Loc.T("track_network");
        WindowTitlesCheck.Content = _h.Loc.T("track_window_titles");
        PrivacyNote.Text = _h.Loc.T("privacy_input_note");
        SaveButton.Content = _h.Loc.T("save");
        CancelButton.Content = _h.Loc.T("cancel");

        SetItemText(LanguageCombo, "system", _h.Loc.T("option_system_language"));
        SetItemText(LanguageCombo, "en", "English");
        SetItemText(LanguageCombo, "ru", "Русский");
        SetItemText(LanguageCombo, "uk", "Українська");
        SetItemText(LanguageCombo, "ja", "日本語");
        SetItemText(LanguageCombo, "zh", "简体中文");
        SetItemText(ThemeCombo, "system", _h.Loc.T("option_system"));
        SetItemText(ThemeCombo, "light", _h.Loc.T("option_light"));
        SetItemText(ThemeCombo, "dark", _h.Loc.T("option_dark"));
        SetItemText(WidgetModeCombo, "compact", _h.Loc.T("option_compact"));
        SetItemText(WidgetModeCombo, "normal", _h.Loc.T("option_normal"));
        SetItemText(WidgetModeCombo, "detailed", _h.Loc.T("option_detailed"));
    }

    private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _h.Loc.Apply(LanguageCodeFromIndex(LanguageCombo.SelectedIndex));
        ApplyLocalization();
        UpdateExportFolderDisplay();
    }

    private void ThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => _h.Theme.Apply(ThemeCodeFromIndex(ThemeCombo.SelectedIndex));

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = _h.Loc.T("choose_export_folder_dialog"),
            UseDescriptionForTitle = true,
            SelectedPath = Directory.Exists(_selectedExportParent)
                ? _selectedExportParent
                : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            _selectedExportParent = OutputFolderService.NormalizeSelectedParent(dialog.SelectedPath);
            UpdateExportFolderDisplay();
        }
    }

    private void UpdateExportFolderDisplay()
    {
        ExportFolderText.Text = Path.Combine(_selectedExportParent, "DayTrack");
        ExportFolderHint.Text = _h.Loc.T("export_folder_hint");
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var x = _h.Settings.Current;
        x.Language = LanguageCodeFromIndex(LanguageCombo.SelectedIndex);
        x.Theme = ThemeCodeFromIndex(ThemeCombo.SelectedIndex);
        x.WidgetMode = WidgetModeCodeFromIndex(WidgetModeCombo.SelectedIndex);
        x.DailyExportParentDirectory = _selectedExportParent;

        if (int.TryParse(AfkTextBox.Text, out var afk))
            x.AfkSeconds = Math.Clamp(afk, 30, 3600);

        x.AutoStart = AutoStartCheck.IsChecked == true;
        x.ShowWidgetOnStartup = ShowWidgetCheck.IsChecked == true;
        x.CreateDesktopShortcut = DesktopShortcutCheck.IsChecked == true;
        x.TrackKeyPresses = KeysCheck.IsChecked == true;
        x.TrackMouseClicks = ClicksCheck.IsChecked == true;
        x.TrackNetwork = NetworkCheck.IsChecked == true;
        x.TrackWindowTitles = WindowTitlesCheck.IsChecked == true;

        _saved = true;
        _h.ApplySettings();
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    private void SettingsWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_saved) return;
        _h.Loc.Apply(_originalLanguage);
        _h.Theme.Apply(_originalTheme);
        _h.Widget?.ApplySettingsAndLocalization();
        _h.Dashboard?.ApplyLocalization();
    }

    private static string LanguageCodeFromIndex(int index)
        => index switch
        {
            1 => "en",
            2 => "ru",
            3 => "uk",
            4 => "ja",
            5 => "zh",
            _ => "system"
        };

    private static string ThemeCodeFromIndex(int index)
        => index switch
        {
            1 => "light",
            2 => "dark",
            _ => "system"
        };

    private static string WidgetModeCodeFromIndex(int index)
        => index switch
        {
            0 => "compact",
            2 => "detailed",
            _ => "normal"
        };

    private static void SetItemText(System.Windows.Controls.ComboBox combo, string tag, string text)
    {
        foreach (var item in combo.Items.OfType<System.Windows.Controls.ComboBoxItem>())
            if ((item.Tag?.ToString() ?? "") == tag) { item.Content = text; return; }
    }

    private static void SelectByTag(System.Windows.Controls.ComboBox combo, string tag)
    {
        foreach (var item in combo.Items.OfType<System.Windows.Controls.ComboBoxItem>())
            if ((item.Tag?.ToString() ?? "") == tag) { combo.SelectedItem = item; return; }
        combo.SelectedIndex = 0;
    }

    private static string SelectedTag(System.Windows.Controls.ComboBox combo, string fallback)
        => (combo.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? fallback;
}
