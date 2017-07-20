using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Konachan.Http;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Konachan.Helper;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace Konachan.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Home : Page
    {
        static int page = 1;
        int count = 0;
        int index = 1;
        bool? State = null;
        string url = string.Empty;
        Load isload = new Load();
        public Home()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            DataContext = isload;
            refreshIndex();
        }

        private void refreshIndex()
        {
            list_index.Items.Clear();
            for (int i = 0; i < 3; i++)
            {
                list_index.Items.Add(index + i);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                if (!string.IsNullOrWhiteSpace(e.Parameter.ToString()))
                {
                    list.Items.Clear();
                    url = "http://konachan.net/post.json?limit=15&page=" + page.ToString() + "&tags=" + e.Parameter.ToString();
                }
            }
            else
            {
                url = "http://konachan.net/post.json?limit=15&page=" + page.ToString();
            }
            if (SettingHelper.ContainsKey("_safe"))
            {
                if ((bool)SettingHelper.GetValue("_safe") == false)
                {
                    State = true;
                    if (SettingHelper.ContainsKey("_r18"))
                    {
                        if ((bool)SettingHelper.GetValue("_r18") == true)
                        {
                            State = false;
                        }
                    }
                }
            }
            list_index.SelectedIndex = 0;
        }
        async Task load()
        {
            try
            {
                list.Items.Clear();
                isload.IsLoading = true;
                string arg = Regex.Match(url, @"&page=\d*").Value;
                string URL = url.Replace(arg, "&page=" + page.ToString());
                List<PostPic> mylist = JsonConvert.DeserializeObject<List<PostPic>>(await BaseService.SentGetAsync(URL));
                switch (State)
                {
                    case null:
                        foreach (var item in mylist)
                        {
                            if (item.Rating == "s")
                                list.Items.Add(item);
                        }
                        break;
                    case true:
                        foreach (var item in mylist)
                        {
                            list.Items.Add(item);
                        }
                        break;
                    case false:
                        foreach (var item in mylist)
                        {
                            //具体分级也不是很清楚，反正不是s（safe）的都偏H便是了
                            if (item.Rating != "s")
                            {
                                list.Items.Add(item);
                                count++;
                            }
                        }
                        if (count < 10)
                        {
                            page++;
                            await load();
                        }
                        else
                        {
                            count = 0;
                        }
                        break;
                }
                isload.IsLoading = false;
            }
            catch (Exception e)
            {
                await popup.Show("错误：" + e.Message);
            }
        }

        private void list_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(PicView), e.ClickedItem as PostPic, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private async void mutiselect_Click(object sender, RoutedEventArgs e)
        {
            list.SelectionMode = list.SelectionMode == ListViewSelectionMode.Multiple 
                ? ListViewSelectionMode.None 
                : ListViewSelectionMode.Multiple;
            list.IsItemClickEnabled = list.SelectionMode == ListViewSelectionMode.Multiple 
                ? false 
                : true;
            if (list.SelectionMode == ListViewSelectionMode.Multiple)
            {
                await popup.Show("点击下载键开始下载");
            }
        }

        private async void download_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            StorageFolder folder = await Methods.GetMyFolderAsync();
            foreach (PostPic item in list.SelectedItems)
            {
                try
                {
                    string name = "konachan_" + item.Id;
                    DownloadOperation download = await DownloadHelper.Download(item.File_url, name + ".png", folder);
                    //如果await，那么执行完第一个StartAsync()后即退出循环.GetCurrentDownloadsAsync()方法同样会遇到此问题.(Download页)
                    IAsyncOperationWithProgress<DownloadOperation, DownloadOperation> start = download.StartAsync();
                    i++;
                }
                catch (Exception err)
                {
                    await new ContentDialog { Content = "错误:" + err.Message, IsSecondaryButtonEnabled = false, PrimaryButtonText = "确定" }.ShowAsync();
                }
            }
            if (i > 0)
            {
                list.SelectionMode = ListViewSelectionMode.None;
                await popup.Show(i.ToString() + "个图片已加入下载队列");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Tag.ToString())
            {
                case "b":
                    {
                        if (index > 1)
                        {
                            index--;
                            refreshIndex();
                            list_index.SelectedIndex = 1;
                        }
                        else
                        {
                            if (list_index.SelectedIndex > 0)
                            {
                                list_index.SelectedIndex--;
                            } 
                        }
                    } break;
                case "f":
                    {
                        if (index > 1)
                        {
                            index++;
                            refreshIndex();
                            list_index.SelectedIndex = 1;
                        }
                        else
                        {
                            if (list_index.SelectedIndex < 2)
                            {
                                list_index.SelectedIndex++;
                            }
                            else
                            {
                                index += 2;
                                refreshIndex();
                                list_index.SelectedIndex = 1;
                            }
                        }
                    } break;
            }
        }

        private async void list_index_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list_index.SelectedItem != null)
            {
                int num= int.Parse(list_index.SelectedItem.ToString());
                page = num;
                if (num > 2)
                {
                    index = num - 1;
                    refreshIndex();
                }
                await load();
            }
        }

        private void goto_Click(object sender, RoutedEventArgs e)
        {
            int num;
            if (!int.TryParse(txt_num.Text, out num)) 
            {
                return;
            }
            if (num < 4)
            {
                index = 1;
                refreshIndex();
                list_index.SelectedIndex = index + num - 1;
            }
            else
            {
                index = num - 1;
                refreshIndex();
                list_index.SelectedIndex = 1;
            }
        }
    }

    public class Load : INotifyPropertyChanged
    {
        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                isLoading = value;
                OnpropertyChanged(nameof(IsLoading));
            }
        }
        private void OnpropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class MyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
