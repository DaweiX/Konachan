using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace Konachan.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DownLoad : Page
    {
        public delegate void SendToast(string title, string content);
        static public event SendToast send;
        List<DownloadOperation> activeDownloads;
        IReadOnlyList<DownloadOperation> downloads;
        CancellationTokenSource cts;
        public DownLoad()
        {
            cts = new CancellationTokenSource();
            this.InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await DiscoverDownloadsAsync();
        }
        private async Task DiscoverDownloadsAsync()
        {
            activeDownloads = new List<DownloadOperation>();
            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
                if (downloads.Count > 0)
                {
                    List<Task> tasks = new List<Task>();
                    foreach (DownloadOperation download in downloads)
                    {
                        HandleModel handModel = new HandleModel
                        {
                            Name = download.ResultFile.Name,
                            DownOpration = download,
                            Size = download.Progress.BytesReceived.ToString(),
                        };
                        if (download.Progress.TotalBytesToReceive > 0)
                        {
                            handModel.Progress = (download.Progress.BytesReceived / download.Progress.TotalBytesToReceive) * 100;
                        }
                        list_now.Items.Add(handModel);
                        tasks.Add(HandleDownloadAsync(handModel));
                    }
                    await Task.WhenAll(tasks);
                }
            }
            catch(Exception e)
            {
                await new ContentDialog { Content = "错误：" + e.ToString(), IsSecondaryButtonEnabled = false, PrimaryButtonText = "确定" }.ShowAsync();
            }
        }

        public class HandleModel : INotifyPropertyChanged
        {
            public CancellationTokenSource cts = new CancellationTokenSource();
            public event PropertyChangedEventHandler PropertyChanged;
            protected void thisPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
            private DownloadOperation downOpration;
            public DownloadOperation DownOpration
            {
                get { return downOpration; }
                set
                {
                    downOpration = value;
                }
            }
            public string Name { get; set; }
            private double progress;
            public double Progress
            {
                get { return progress; }
                set
                {
                    if (downOpration.Progress.TotalBytesToReceive != 0)
                    {
                        progress = value;
                        thisPropertyChanged("Progress");
                    }
                }
            }

            private string size;
            public string Size
            {
                get { return size; }
                set
                {
                    if (downOpration.Progress.TotalBytesToReceive == 0)
                    {
                        size = "连接地址中，请稍候";
                    }
                    else
                    {
                        size = (Convert.ToDouble(value) / 1024 / 1024).ToString("0.0") + "M/" + ((double)downOpration.Progress.TotalBytesToReceive / 1024 / 1024).ToString("0.0") + "M";
                    }
                    thisPropertyChanged("Size");
                }
            }
            public string Guid { get { return downOpration.Guid.ToString(); } }
            public string status;
            public string Status
            {
                get { thisPropertyChanged("Status"); return status; }
                set
                {
                    switch (downOpration.Progress.Status)
                    {
                        case BackgroundTransferStatus.Idle:
                            status = "空闲中";
                            break;
                        case BackgroundTransferStatus.Running:
                            status = "下载中";
                            break;
                        case BackgroundTransferStatus.PausedByApplication:
                            status = "已暂停";
                            break;
                        case BackgroundTransferStatus.PausedCostedNetwork:
                            status = "使用数据流量，暂停下载";
                            break;
                        case BackgroundTransferStatus.PausedNoNetwork:
                            status = "网络断开";
                            break;
                        case BackgroundTransferStatus.Completed:
                            status = "完成";
                            break;
                        case BackgroundTransferStatus.Canceled:
                            status = "取消";
                            break;
                        case BackgroundTransferStatus.Error:
                            status = "下载错误";
                            break;
                        case BackgroundTransferStatus.PausedSystemPolicy:
                            status = "因系统资源受限暂停";
                            break;
                    }
                    thisPropertyChanged("Status");
                }
            }
        }

        private async Task HandleDownloadAsync(HandleModel model)
        {
            var download = model.DownOpration;
            try
            {
                DownLoadProgress(download);
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownLoadProgress);
                await download.AttachAsync().AsTask(cts.Token, progressCallback);
            }
            catch (TaskCanceledException)
            {
                // messagepop.Show("取消： " + download.Guid);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void Dispose()
        {
            if (cts != null)
            {
                cts.Dispose();
                cts = null;
            }
            GC.SuppressFinalize(this);
        }

        private void DownLoadProgress(DownloadOperation download)
        {
            try
            {
                HandleModel test = null;
                foreach (HandleModel item in list_now.Items)
                {
                    if (item.DownOpration.Guid == download.Guid)
                    {
                        test = item;
                    }
                }
                if (list_now.Items.Contains(test))
                {
                    HandleModel item= (HandleModel)list_now.Items[list_now.Items.IndexOf(test)];
                    item.Size = download.Progress.BytesReceived.ToString();
                    item.Status = download.Progress.Status.ToString();
                    item.Progress = ((double)download.Progress.BytesReceived / download.Progress.TotalBytesToReceive) * 100;
                    if (item.Progress == 100 && download.Progress.BytesReceived > 0) 
                    {
                        //Sendtoast("下载完成", ((HandleModel)list_now.Items[list_now.Items.IndexOf(test)]).Name);
                        send("下载完成", ((HandleModel)list_now.Items[list_now.Items.IndexOf(test)]).Name);
                        list_now.Items.Remove(test);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        public class MyProgress : INotifyPropertyChanged
        {
            private int progress;
            public string Name { get; set; }
            public int Progress
            {
                get
                {
                    return progress;
                }
                set
                {
                    progress = value;
                    OnPropertyChanged("Progress");
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        HandleModel model = new HandleModel();
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
           // flyout.ShowAt(list_now, e.GetPosition(list_now));
           // model = (sender as Grid).DataContext as HandleModel;
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (list_now.SelectedItems.Count > 0)
                {
                    foreach (HandleModel item in list_now.SelectedItems)
                    {
                        item.DownOpration.Pause();
                    }
                }
            }
            catch { }
        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (HandleModel item in list_now.SelectedItems)
                {
                    item.DownOpration.Resume();
                }
            }
            catch { }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void pr_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
