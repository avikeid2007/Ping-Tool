using Microsoft.UI.Xaml.Controls;

namespace PingTool.Helpers;

public static class NavHelper
{
    public static Type GetNavigateTo(NavigationViewItem item) =>
        (Type)item.GetValue(NavigateToProperty);

    public static void SetNavigateTo(NavigationViewItem item, Type value) =>
        item.SetValue(NavigateToProperty, value);

    public static readonly Microsoft.UI.Xaml.DependencyProperty NavigateToProperty =
        Microsoft.UI.Xaml.DependencyProperty.RegisterAttached(
            "NavigateTo",
            typeof(Type),
            typeof(NavHelper),
            new Microsoft.UI.Xaml.PropertyMetadata(null));
}
