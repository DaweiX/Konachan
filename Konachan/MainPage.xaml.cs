using Konachan.Helper;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace Konachan
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool isExit = false;
        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
            mainframe.Navigate(typeof(Views.Home));
            TitleBarInit();
            MainPage_switchNight();
            Views.DownLoad.send += SendToast;
        }

        private void SendToast(string title, string content)
        {
            var tmp = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var txtNodes = tmp.GetElementsByTagName("text");
            if (!(txtNodes == null || txtNodes.Length == 0))
            {
                var txtnode = txtNodes[0];
                txtnode.InnerText = title + Environment.NewLine + content;
                ToastNotification toast = new ToastNotification(tmp);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
        }
    

        private void TitleBarInit()
        {
            var TitleBar = ApplicationView.GetForCurrentView().TitleBar;
            Color color = Color.FromArgb(255, 226, 115, 169);
            TitleBar.BackgroundColor = color;
            TitleBar.ButtonBackgroundColor = color;
            TitleBar.ForegroundColor = Colors.White;
            TitleBar.ButtonHoverForegroundColor = Colors.Black;
            TitleBar.ButtonHoverBackgroundColor = Colors.White;
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar sb = StatusBar.GetForCurrentView();
                sb.BackgroundColor = color;
                sb.BackgroundOpacity = 1;
            }
        }
        //后退键
        private async void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (mainframe == null)
                return;
            if (e.Handled == false)
            {
                if (mainframe.CanGoBack)
                {
                    e.Handled = true;
                    mainframe.GoBack();
                }
                else
                {
                    if (isExit)
                    {
                        Application.Current.Exit();
                    }
                    else
                    {
                        e.Handled = true;
                        isExit = true;
                        await Task.Delay(2000);
                        isExit = false;
                    }
                }
            }
        }
        private void Border_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
            ham.IsPaneOpen = !ham.IsPaneOpen;
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            mainframe.Navigate(typeof(Views.Home), SearchBox.Text);
        }

        private void mainframe_Navigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = mainframe.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            switch (mainframe.CurrentSourcePageType.AssemblyQualifiedName.Split(',')[0].Split('.')[2])
            {
                case "Setting": (mainframe.Content as Views.Setting).switchNight += MainPage_switchNight; break;
                case "PicView": (mainframe.Content as Views.PicView).GotoSearch += MainPage_GotoSearch;break;
            }
        }

        private void MainPage_GotoSearch(string tag)
        {
            SearchBox.Text = tag;
            mainframe.Navigate(typeof(Views.Home), SearchBox.Text);
        }

        private void MainPage_switchNight()
        {
            if (SettingHelper.ContainsKey("_night"))
            {
                RequestedTheme = (bool)SettingHelper.GetValue("_night") ? ElementTheme.Dark : ElementTheme.Light;
            }
        }

        private void MainList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Sets.SelectedIndex = -1;
            if (ham.DisplayMode == SplitViewDisplayMode.Overlay)
                ham.IsPaneOpen = false;
            switch (MainList.SelectedIndex)
            {
                case 0:
                    {
                        mainframe.Navigate(typeof(Views.Home));
                    }
                    break;
                case 1:
                    {
                        mainframe.Navigate(typeof(Views.DownLoad));
                    }
                    break;
                case 2:
                    {
                        mainframe.Navigate(typeof(Views.Local));
                    }
                    break;
            }
        }

        private void Sets_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ham.DisplayMode == SplitViewDisplayMode.Overlay)
                ham.IsPaneOpen = false;
            MainList.SelectedIndex = -1;
            mainframe.Navigate(typeof(Views.Setting));
        }
    }
}
