using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Konachan.Http;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.Storage.Provider;
using Konachan.Helper;
using Windows.Networking.BackgroundTransfer;
using Windows.Foundation;
using Windows.ApplicationModel.DataTransfer;
using System.IO;
using Windows.System;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace Konachan.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PicView : Page
    {
        public delegate void Search(string tag);
        public event Search GotoSearch;
        bool isLoad = false;
        bool? isLocal = null;
        string ID = string.Empty;
        PostPic Pic = new PostPic();
        StorageFile file = null;
        public PicView()
        {
            InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await popup.Show("轻触屏幕以查看图片信息");
            if (e.Parameter.GetType() == typeof(PostPic))
            {
                isLocal = false;
                Pic = e.Parameter as PostPic;
                ID = Pic.Id;
                BitmapImage bmp = new BitmapImage();
                BitmapImage bmp0 = new BitmapImage();
                bmp.UriSource = new Uri(Pic.Jpeg_url);
                bmp0.UriSource = new Uri(Pic.Preview_url);
                img.Source = bmp;
                img0.Source = bmp0;
                img.ImageOpened += Img_ImageOpened;
                img0.Visibility = Visibility.Visible;
                ring.Visibility = Visibility.Visible;
            }
            else if(e.Parameter.GetType() == typeof(StorageFile))
            {
                file = e.Parameter as StorageFile;
                isLocal = true;
                Stream stream = await file.OpenStreamForReadAsync();
                var randomAccessStream = new InMemoryRandomAccessStream();
                var outputStream = randomAccessStream.GetOutputStreamAt(0);
                BitmapImage bmp = new BitmapImage();
                await RandomAccessStream.CopyAsync(stream.AsInputStream(), outputStream);
                await bmp.SetSourceAsync(randomAccessStream);
                img.Source = bmp;
                ring.Visibility = Visibility.Collapsed;
                img0.Visibility = Visibility.Collapsed;
            }
        }

        private void Img_ImageOpened(object sender, RoutedEventArgs e)
        {
            img0.Visibility = Visibility.Collapsed;
            ring.Visibility = Visibility.Collapsed;
        }

        async Task SavePicture()
        {
            FileSavePicker picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeChoices.Add("图片", new List<string>() { ".jpg" });
            picker.SuggestedFileName = "konachan_" + Pic.Id + "_jpg";
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                IBuffer buffer = await DownloadHelper.GetBuffer(Pic.Jpeg_url);
                CachedFileManager.DeferUpdates(file);
                await FileIO.WriteBufferAsync(file, buffer);
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                await popup.Show("保存成功");
            }
        }

        private async void img_Tapped(object sender, TappedRoutedEventArgs e)
        {
            splitview.IsPaneOpen = !splitview.IsPaneOpen;
            if (!isLoad)
            {
                if (isLocal == false)
                {
                    ID = txt_id.Text = Pic.Id;
                    txt_height.Text = Pic.Height.ToString();
                    txt_width.Text = Pic.Width.ToString();
                    txt_md5.Text = Pic.MD5;
                    txt_addr.Text = Pic.Source;
                    txt_time.Text = Pic.Created_at;
                    tags.ItemsSource = Pic.Tags.Split(' ');
                    txt_size.Text = (double.Parse(Pic.File_size) / Math.Pow(1024, 2)).ToString("0.00") + "MB";
                    isLoad = true;
                }
                else if (isLocal == true && file != null)
                {
                    header_md5.Visibility = Visibility.Collapsed;
                    txt_md5.Visibility = Visibility.Collapsed;
                    btns.Visibility = Visibility.Collapsed;
                    header_tags.Visibility = Visibility.Collapsed;
                    header_time.Text = "修改时间:";
                    txt_id.Text = file.DisplayName.Split('_')[1];
                    var property = await file.Properties.GetImagePropertiesAsync();
                    var basic = await file.GetBasicPropertiesAsync();
                    txt_height.Text = property.Height.ToString();
                    txt_width.Text = property.Width.ToString();
                    txt_size.Text = (basic.Size / Math.Pow(1024, 2)).ToString("0.0") + "MB";
                    txt_addr.Text = file.Path;
                    txt_time.Text = basic.DateModified.ToLocalTime().ToString();
                    isLoad = true;
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await SavePicture();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = "konachan_" + Pic.Id;
                StorageFolder folder = await Methods.GetMyFolderAsync();
                DownloadOperation download = await DownloadHelper.Download(Pic.File_url, name + Pic.File_url.Substring(Pic.File_url.Length - 3), folder);
                //如果await，那么执行完第一个StartAsync()后即退出循环.GetCurrentDownloadsAsync()方法同样会遇到此问题.(Download页)
                IAsyncOperationWithProgress<DownloadOperation, DownloadOperation> start = download.StartAsync();
                await popup.Show("图片已加入下载队列");
            }
            catch (Exception err)
            {
                await popup.Show("错误:" + err.Message);
            }
        }

        private void tags_ItemClick(object sender, ItemClickEventArgs e)
        {
            GotoSearch(e.ClickedItem.ToString().Replace(' ', '_'));
        }

        private async void share_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            switch (item.Tag.ToString())
            {
                case "0":
                    {
                        DataPackage pack = new DataPackage();
                        pack.SetText(string.Format("http://konachan.net/post/show/{0}/", ID));
                        Clipboard.SetContent(pack);
                        Clipboard.Flush();
                        await popup.Show("已将链接复制至剪贴板");
                    }
                    break;
                case "1":
                    {
                        DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                        dataTransferManager.DataRequested += DataTransferManager_DataRequested;
                        DataTransferManager.ShowShareUI();
                    }
                    break;
            }
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = "来自Konachan的分享";
            request.Data.Properties.Description = "分享当前图片";
            //IRandomAccessStreamReference bitmapRef = await new BitmapImage(new Uri(details.Pic));
            request.Data.SetText(string.Format("我在Konachan上向你推荐一张图片，\n链接：http://konachan.net/post/show/{0}/", ID));

        }

        private async void web_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(string.Format("http://konachan.net/post/show/{0}/", ID)));
        }
    }
}
