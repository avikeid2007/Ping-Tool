using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace PingTool.Views;

public sealed partial class SuggestionPage : Page
{
    public SuggestionPage()
    {
        InitializeComponent();
    }

    private async void SendFeedback_Click(object sender, RoutedEventArgs e)
    {
        var suggestion = SuggestionText.Text;
        
        if (string.IsNullOrWhiteSpace(suggestion))
        {
            var dialog = new ContentDialog
            {
                Title = "Empty Feedback",
                Content = "Please enter your suggestion or feedback.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
            return;
        }

        // Open email with pre-filled content
        var emailUri = new Uri($"mailto:avikeid2007@gmail.com?subject=Ping%20Legacy%20Feedback&body={Uri.EscapeDataString(suggestion)}");
        await Launcher.LaunchUriAsync(emailUri);

        // Show confirmation
        var confirmDialog = new ContentDialog
        {
            Title = "Thank You!",
            Content = "Your feedback has been opened in your email client.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await confirmDialog.ShowAsync();

        SuggestionText.Text = string.Empty;
    }
}
