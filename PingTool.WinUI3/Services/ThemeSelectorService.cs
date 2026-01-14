using Microsoft.UI.Xaml;

namespace PingTool.Services;

public static class ThemeSelectorService
{
    private const string SettingsKey = "AppBackgroundRequestedTheme";
    private static ElementTheme _theme = ElementTheme.Default;

    public static ElementTheme Theme
    {
        get => _theme;
        set
        {
            _theme = value;
            SetRequestedTheme();
        }
    }

    public static void Initialize()
    {
        var savedTheme = SettingsHelper.Read<string>(SettingsKey);
        if (!string.IsNullOrEmpty(savedTheme) && Enum.TryParse<ElementTheme>(savedTheme, out var theme))
        {
            _theme = theme;
        }
        SetRequestedTheme();
    }

    public static void SetRequestedTheme()
    {
        if (App.MainWindow?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = _theme;
        }
        SettingsHelper.Save(SettingsKey, _theme.ToString());
    }
}
