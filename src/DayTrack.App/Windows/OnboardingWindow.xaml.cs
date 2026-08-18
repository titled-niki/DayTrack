using DayTrack.Services;
using System.Windows;
using System.Windows.Controls;

namespace DayTrack.Windows;

public partial class OnboardingWindow : Window
{
    private readonly AppHost _h;
    private string _selectedExportParent;

    public bool LaunchWidgetAfterFinish { get; private set; } = true;

    public OnboardingWindow(AppHost h)
    {
        InitializeComponent();
        _h = h;

        _selectedExportParent =
            OutputFolderService.NormalizeSelectedParent(
                h.Settings.Current.DailyExportParentDirectory);

        SelectLanguage(h.Settings.Current.Language);
        SelectTheme(h.Settings.Current.Theme);

        AutoStartCheck.IsChecked = true;
        DesktopShortcutCheck.IsChecked = true;
        LaunchAfterSetupCheck.IsChecked = true;
        KeysCheck.IsChecked = true;
        ClicksCheck.IsChecked = true;

        ApplyCurrentLanguage();
        ApplyCurrentTheme();
        ApplyText();
        UpdateExportFolderDisplay();

        LanguageCombo.SelectionChanged += LanguageCombo_SelectionChanged;
        ThemeCombo.SelectionChanged += ThemeCombo_SelectionChanged;
    }

    private void LanguageCombo_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        ApplyCurrentLanguage();
        ApplyText();
        UpdateExportFolderDisplay();
    }

    private void ThemeCombo_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        ApplyCurrentTheme();
    }

    private void ApplyCurrentLanguage()
    {
        var code = LanguageCodeFromIndex(LanguageCombo.SelectedIndex);
        _h.Loc.Apply(code);

        RuntimeLog.Write(
            $"Onboarding language selection | index={LanguageCombo.SelectedIndex} | code={code} | resolved={_h.Loc.CurrentLanguage}");
    }

    private void ApplyCurrentTheme()
    {
        var code = ThemeCodeFromIndex(ThemeCombo.SelectedIndex);
        _h.Theme.Apply(code);
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = _h.Loc.T("choose_export_folder_dialog"),
            UseDescriptionForTitle = true,
            SelectedPath = Directory.Exists(_selectedExportParent)
                ? _selectedExportParent
                : Environment.GetFolderPath(
                    Environment.SpecialFolder.DesktopDirectory),
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            _selectedExportParent =
                OutputFolderService.NormalizeSelectedParent(
                    dialog.SelectedPath);

            UpdateExportFolderDisplay();
        }
    }

    private void UpdateExportFolderDisplay()
    {
        ExportFolderText.Text =
            Path.Combine(_selectedExportParent, "DayTrack");

        ExportFolderHint.Text =
            _h.Loc.T("export_folder_hint");
    }

    private void ApplyText()
    {
        TaglineText.Text = _h.Loc.T("product_tagline");
        IntroText.Text = _h.Loc.T("onboarding_intro");
        LanguageLabel.Text = _h.Loc.T("language");
        ThemeLabel.Text = _h.Loc.T("theme");

        ExportFolderLabel.Text =
            _h.Loc.T("daily_reports_folder");

        BrowseFolderButton.Content =
            _h.Loc.T("browse");

        AutoStartCheck.Content =
            _h.Loc.T("autostart");

        DesktopShortcutCheck.Content =
            _h.Loc.T("create_desktop_shortcut");

        LaunchAfterSetupCheck.Content =
            _h.Loc.T("launch_after_setup");

        KeysCheck.Content =
            _h.Loc.T("count_keys");

        ClicksCheck.Content =
            _h.Loc.T("count_clicks");

        PrivacyNote.Text =
            _h.Loc.T("privacy_input_note");

        FinishButton.Content =
            _h.Loc.T("finish");

        SetItemText(
            LanguageCombo,
            0,
            _h.Loc.T("option_system_language"));

        SetItemText(LanguageCombo, 1, "English");
        SetItemText(LanguageCombo, 2, "Русский");
        SetItemText(LanguageCombo, 3, "Українська");
        SetItemText(LanguageCombo, 4, "日本語");
        SetItemText(LanguageCombo, 5, "简体中文");

        SetItemText(
            ThemeCombo,
            0,
            _h.Loc.T("option_system"));

        SetItemText(
            ThemeCombo,
            1,
            _h.Loc.T("option_light"));

        SetItemText(
            ThemeCombo,
            2,
            _h.Loc.T("option_dark"));
    }

    private void Finish_Click(object sender, RoutedEventArgs e)
    {
        // Read the selection DIRECTLY at the moment Finish is pressed.
        // This cannot fall back to an old Russian value from settings/event state.
        var selectedLanguage =
            LanguageCodeFromIndex(LanguageCombo.SelectedIndex);

        var selectedTheme =
            ThemeCodeFromIndex(ThemeCombo.SelectedIndex);

        RuntimeLog.Write(
            $"Finish pressed | languageIndex={LanguageCombo.SelectedIndex} | language={selectedLanguage} | theme={selectedTheme}");

        var settings = _h.Settings.Current;

        settings.Language = selectedLanguage;
        settings.Theme = selectedTheme;
        settings.DailyExportParentDirectory = _selectedExportParent;

        settings.AutoStart =
            AutoStartCheck.IsChecked == true;

        settings.CreateDesktopShortcut =
            DesktopShortcutCheck.IsChecked == true;

        settings.LaunchWidgetAfterSetup =
            LaunchAfterSetupCheck.IsChecked == true;

        settings.TrackKeyPresses =
            KeysCheck.IsChecked == true;

        settings.TrackMouseClicks =
            ClicksCheck.IsChecked == true;

        settings.FirstRun = false;

        LaunchWidgetAfterFinish =
            settings.LaunchWidgetAfterSetup;

        // Apply the exact language before any report is generated.
        _h.Loc.Apply(selectedLanguage);
        _h.Theme.Apply(selectedTheme);

        _h.ApplySettings();

        // Explicitly verify creation now, not silently later.
        try
        {
            var report = _h.Export.DailyTxt(DateTime.Today);

            RuntimeLog.Write(
                $"Initial report verified | setting={settings.Language} | resolved={_h.Loc.CurrentLanguage} | path={report}");
        }
        catch (Exception ex)
        {
            RuntimeLog.WriteException("Initial report creation failed", ex);

            System.Windows.MessageBox.Show(
                $"DayTrack could not create today's report.\n\n{ex.Message}\n\nLog:\n{RuntimeLog.FilePath}",
                "DayTrack",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        DialogResult = true;
        Close();
    }

    private void SelectLanguage(string? code)
    {
        LanguageCombo.SelectedIndex =
            (code ?? "system").ToLowerInvariant() switch
            {
                "en" => 1,
                "ru" => 2,
                "uk" => 3,
                "ja" => 4,
                "zh" => 5,
                _ => 0
            };
    }

    private void SelectTheme(string? code)
    {
        ThemeCombo.SelectedIndex =
            (code ?? "system").ToLowerInvariant() switch
            {
                "light" => 1,
                "dark" => 2,
                _ => 0
            };
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

    private static void SetItemText(
        System.Windows.Controls.ComboBox combo,
        int index,
        string text)
    {
        if (index < 0 || index >= combo.Items.Count)
            return;

        if (combo.Items[index] is System.Windows.Controls.ComboBoxItem item)
            item.Content = text;
    }
}
