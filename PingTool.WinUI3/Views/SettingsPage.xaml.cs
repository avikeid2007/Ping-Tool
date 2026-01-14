using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PingTool.Services;

namespace PingTool.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Set current theme selection
        var currentTheme = ThemeSelectorService.Theme;
        foreach (var item in ThemeRadioButtons.Items)
        {
            if (item is RadioButton rb && rb.Tag?.ToString() == currentTheme.ToString())
            {
                rb.IsChecked = true;
                break;
            }
        }

        // Set auto-start toggle
        var isPingAutoStart = SettingsHelper.Read<bool?>("IsPingAutoStart") ?? true;
        AutoStartToggle.IsOn = isPingAutoStart;
    }

    private void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeRadioButtons.SelectedItem is RadioButton rb && rb.Tag is string themeTag)
        {
            if (Enum.TryParse<ElementTheme>(themeTag, out var theme))
            {
                ThemeSelectorService.Theme = theme;
            }
        }
    }

    private void AutoStartToggle_Toggled(object sender, RoutedEventArgs e)
    {
        SettingsHelper.Save("IsPingAutoStart", AutoStartToggle.IsOn);
    }
}
