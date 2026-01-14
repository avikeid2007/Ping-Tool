using Microsoft.UI.Xaml.Controls;

namespace PingTool.Services;

public static class NavigationService
{
    private static Frame? _frame;

    public static Frame? Frame
    {
        get => _frame;
        set => _frame = value;
    }

    public static bool CanGoBack => Frame?.CanGoBack ?? false;

    public static void GoBack()
    {
        if (CanGoBack)
        {
            Frame?.GoBack();
        }
    }

    public static bool Navigate(Type pageType, object? parameter = null)
    {
        if (Frame == null) return false;
        return Frame.Navigate(pageType, parameter);
    }

    public static bool Navigate<T>(object? parameter = null) where T : Page
    {
        return Navigate(typeof(T), parameter);
    }
}
