using System;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PingTool.Views
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel ViewModel;
        public MainPage()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            this.DataContext = ViewModel;
        }

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }


        private async void Button_Click_1(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ValueSet request = new ValueSet();
            request.Add("host", "8.8.8.8");
            request.Add("isStop", "false");
            await App.Connection.SendMessageAsync(request);
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync(e);
            base.OnNavigatedTo(e);
        }
        //private async void Reconnect()
        //{
        //    if (App.IsForeground)
        //    {
        //        MessageDialog dlg = new MessageDialog("Connection to desktop process lost. Reconnect?");
        //        UICommand yesCommand = new UICommand("Yes", async (r) =>
        //        {
        //            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
        //        });
        //        dlg.Commands.Add(yesCommand);
        //        UICommand noCommand = new UICommand("No", (r) => { });
        //        dlg.Commands.Add(noCommand);
        //        await dlg.ShowAsync();
        //    }
        //}

    }
}
