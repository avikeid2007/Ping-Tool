using Microsoft.UI.Xaml;

namespace PingTool;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Set window title
        Title = "Ping Legacy";
        
        // Store reference to main window for later use
        App.MainWindow = this;
    }
}
