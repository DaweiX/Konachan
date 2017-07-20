using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Konachan.Helper;
using Windows.System;
using System;
using Windows.Storage;
using Windows.Storage.Pickers;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace Konachan.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Setting : Page
    {
        public delegate void ChangeTheme();
        public event ChangeTheme switchNight;
        public Setting()
        {
            this.InitializeComponent();
            Init();
        }

        private void Init()
        {
            if (SettingHelper.ContainsKey("_downloadcost"))
            {
                cost.IsOn = (bool)SettingHelper.GetValue("_downloadcost");
            }
            if (SettingHelper.ContainsKey("_safe"))
            {
                safe.IsOn = (bool)SettingHelper.GetValue("_safe");
            }
            if (SettingHelper.ContainsKey("_r18"))
            {
                R18.IsOn = (bool)SettingHelper.GetValue("_r18");
            }
            if (SettingHelper.ContainsKey("_night"))
            {
                night.IsOn = (bool)SettingHelper.GetValue("_night");
            }
            if (SettingHelper.ContainsKey("_path"))
            {
                if (!string.IsNullOrEmpty(SettingHelper.GetValue("_path").ToString())) 
                {
                    path.IsOn = true;
                    txt_path.Text = SettingHelper.GetValue("_path").ToString();
                }
                else
                {
                    path.IsOn = false;
                    txt_path.Text = @"图片库\Konachan";
                }
            }
            else
            {
                path.IsOn = false;
                txt_path.Text = @"图片库\Konachan";
            }
            R18.Visibility = safe.IsOn ? Visibility.Collapsed : Visibility.Visible;
        }

        private void cost_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_downloadcost", cost.IsOn);
        }

        private void R18_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_r18", R18.IsOn);
        }

        private void safe_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_safe", safe.IsOn);
            R18.Visibility = safe.IsOn ? Visibility.Collapsed : Visibility.Visible;
        }

        private void night_Toggled(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("_night", night.IsOn);
            switchNight?.Invoke();
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("mailto:DaweiX@outlook.com"));

        }

        private async void path_Toggled(object sender, RoutedEventArgs e)
        {
            if (path.IsOn == false)
            {
                //if (!SettingHelper.ContainsKey("_path"))
                //{
                //    return;
                //}
                //if (string.IsNullOrEmpty(SettingHelper.GetValue("_path").ToString()))
                //{
                //    return;
                //}
                SettingHelper.SetValue("_path", string.Empty);
                txt_path.Text = @"图片库\Konachan";
                return;
            }
            StorageFolder folder;
            FolderPicker picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                string path = folder.Path;
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                SettingHelper.SetValue("_path", path);
                txt_path.Text = path;
            }
            else
            {
                path.IsOn = false;
            }
        }
    }
}
