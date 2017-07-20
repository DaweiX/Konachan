using Konachan.Helper;
using System;
using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace Konachan.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Local : Page
    {
        public Local()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            StorageFolder folder = await Methods.GetMyFolderAsync();
            foreach (var file in await folder.GetFilesAsync())
            {
                try
                {
                    var thumbnail = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView);
                    Stream stream = thumbnail.AsStreamForRead();
                    var randomAccessStream = new InMemoryRandomAccessStream();
                    var outputStream = randomAccessStream.GetOutputStreamAt(0);
                    BitmapImage bmp = new BitmapImage();
                    await RandomAccessStream.CopyAsync(stream.AsInputStream(), outputStream);
                    await bmp.SetSourceAsync(randomAccessStream);
                    list.Items.Add(new LocalPic
                    {
                        Thumbnail = bmp,
                        ID = file.DisplayName.Split('_')[1],
                        File = file,
                    });
                }
                catch { }
            }

        }

        class LocalPic
        {
            public string ID { get; set; }
            public ImageSource Thumbnail { get; set; }
            public StorageFile File { get; set; }
        }

        private void list_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(PicView), (e.ClickedItem as LocalPic).File, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
        }

        private void mutiselect_Click(object sender, RoutedEventArgs e)
        {
            list.SelectionMode = list.SelectionMode == ListViewSelectionMode.Multiple ? ListViewSelectionMode.None : ListViewSelectionMode.Multiple;
            list.IsItemClickEnabled = list.SelectionMode == ListViewSelectionMode.Multiple ? false : true;
        }

        private async void del_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await Methods.GetMyFolderAsync();
            foreach (LocalPic Pic in list.SelectedItems)
            {
                StorageFile file = await folder.GetFileAsync(Pic.File.Name);
                await file.DeleteAsync();
                list.Items.Remove(Pic);
            }
            list.SelectionMode = ListViewSelectionMode.None;
        }
    }
}
