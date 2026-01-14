using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PingTool.Helpers;
using PingTool.Services;
using Windows.System;

namespace PingTool.Views;

public sealed partial class ShellPage : Page
{
    public ShellPage()
    {
        InitializeComponent();
        NavigationService.Frame = shellFrame;
        
        // Navigate to main page on load
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        NavigationService.Navigate(typeof(MainPage));
    }

    public bool IsBackEnabled => NavigationService.CanGoBack;

    public object? Selected => navigationView.SelectedItem;

    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            NavigationService.Navigate(typeof(SettingsPage));
        }
        else if (args.InvokedItemContainer is NavigationViewItem item)
        {
            var pageType = NavHelper.GetNavigateTo(item);
            if (pageType != null)
            {
                NavigationService.Navigate(pageType);
            }
        }
    }

    private async void RateUs_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9P1KVKT59T2M"));
    }
}
