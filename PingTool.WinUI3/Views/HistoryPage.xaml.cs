using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PingTool.Helpers;

namespace PingTool.Views;

public sealed partial class HistoryPage : Page
{
    public HistoryPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        LoadHistory();
    }

    private void LoadHistory()
    {
        var history = SQLiteHelper.GetAllDistinct().ToList();
        HistoryList.ItemsSource = history;
        EmptyMessage.Visibility = history.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void ViewDetails_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid pingId)
        {
            var details = SQLiteHelper.GetAll(pingId).ToList();
            var content = string.Join("\n", details.Select(d => d.Response));

            var dialog = new ContentDialog
            {
                Title = "Ping Details",
                Content = new ScrollViewer
                {
                    Content = new TextBlock { Text = content, TextWrapping = TextWrapping.Wrap },
                    MaxHeight = 400
                },
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
