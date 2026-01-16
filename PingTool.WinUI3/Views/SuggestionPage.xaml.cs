using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PingTool.Helpers;
using System.Text;
using Windows.ApplicationModel;
using Windows.System;

namespace PingTool.Views;

public sealed partial class SuggestionPage : Page
{
    private const string GitHubRepoUrl = "https://github.com/avikeid2007/Ping-Tool";

    public SuggestionPage()
    {
        InitializeComponent();
    }

    private async void OpenGitHubIssue_Click(object sender, RoutedEventArgs e)
    {
        var title = TitleText.Text.Trim();
        var description = DescriptionText.Text.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            await ShowDialog("Title Required", "Please enter a title for your feedback.");
            return;
        }

        // Build issue body
        var body = new StringBuilder();
        body.AppendLine("## Description");
        body.AppendLine(string.IsNullOrWhiteSpace(description) ? "_No description provided_" : description);
        body.AppendLine();

        if (IncludeSystemInfo.IsChecked == true)
        {
            body.AppendLine("## System Information");
            body.AppendLine($"- **App Version**: {GetAppVersion()}");
            body.AppendLine($"- **OS**: {Environment.OSVersion}");
            body.AppendLine($"- **.NET Version**: {Environment.Version}");
            body.AppendLine($"- **Machine**: {Environment.MachineName}");
            body.AppendLine();
        }

        // Determine label based on feedback type
        var labels = new List<string>();
        if (FeatureRadio.IsChecked == true) labels.Add("enhancement");
        else if (BugRadio.IsChecked == true) labels.Add("bug");
        else if (QuestionRadio.IsChecked == true) labels.Add("question");

        // Build GitHub new issue URL
        var issueUrl = $"{GitHubRepoUrl}/issues/new?" +
                      $"title={Uri.EscapeDataString(title)}" +
                      $"&body={Uri.EscapeDataString(body.ToString())}" +
                      (labels.Count > 0 ? $"&labels={string.Join(",", labels)}" : "");

        await Launcher.LaunchUriAsync(new Uri(issueUrl));

        // Clear form
        TitleText.Text = string.Empty;
        DescriptionText.Text = string.Empty;
    }

    private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
    {
        var content = BuildFeedbackText();
        FileHelper.CopyText(content);
    }

    private string BuildFeedbackText()
    {
        var sb = new StringBuilder();
        
        var feedbackType = FeatureRadio.IsChecked == true ? "Feature Request" :
                          BugRadio.IsChecked == true ? "Bug Report" : "Question";
        
        sb.AppendLine($"# {feedbackType}: {TitleText.Text}");
        sb.AppendLine();
        sb.AppendLine("## Description");
        sb.AppendLine(DescriptionText.Text);
        sb.AppendLine();

        if (IncludeSystemInfo.IsChecked == true)
        {
            sb.AppendLine("## System Information");
            sb.AppendLine($"- App Version: {GetAppVersion()}");
            sb.AppendLine($"- OS: {Environment.OSVersion}");
            sb.AppendLine($"- .NET Version: {Environment.Version}");
        }

        return sb.ToString();
    }

    private static string GetAppVersion()
    {
        try
        {
            var version = Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
        catch
        {
            return "Unknown";
        }
    }

    private async Task ShowDialog(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }
}
