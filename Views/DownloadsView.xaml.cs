using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WallpaperClient.Services;

namespace WallpaperClient.Views
{
    /// <summary>
    /// DownloadsView.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadsView : UserControl
    {
        public DownloadsView()
        {
            InitializeComponent();
            Loaded += DownloadsView_Loaded;
        }

        private void DownloadsView_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateEmptyState();
        }

        /// <summary>
        /// 更新空状态显示
        /// </summary>
        private void UpdateEmptyState()
        {
            if (DataContext is ViewModels.MainViewModel viewModel)
            {
                EmptyStateBorder.Visibility = viewModel.DownloadTasks.Count == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 暂停/恢复按钮点击事件
        /// </summary>
        private void PauseResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string downloadId)
            {
                if (DataContext is ViewModels.MainViewModel viewModel)
                {
                    var task = viewModel.DownloadTasks.FirstOrDefault(t => t.Id == downloadId);
                    if (task != null)
                    {
                        if (task.State == DownloadState.Downloading)
                        {
                            viewModel.PauseDownloadCommand.Execute(downloadId);
                        }
                        else if (task.State == DownloadState.Paused)
                        {
                            viewModel.ResumeDownloadCommand.Execute(downloadId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string downloadId)
            {
                if (DataContext is ViewModels.MainViewModel viewModel)
                {
                    viewModel.CancelDownloadCommand.Execute(downloadId);
                }
            }
        }

        /// <summary>
        /// 删除按钮点击事件
        /// </summary>
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string downloadId)
            {
                if (DataContext is ViewModels.MainViewModel viewModel)
                {
                    viewModel.RemoveDownloadCommand.Execute(downloadId);
                    UpdateEmptyState();
                }
            }
        }

        /// <summary>
        /// 全部暂停按钮点击事件
        /// </summary>
        private void PauseAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel viewModel)
            {
                foreach (var task in viewModel.DownloadTasks)
                {
                    if (task.State == DownloadState.Downloading)
                    {
                        viewModel.PauseDownloadCommand.Execute(task.Id);
                    }
                }
            }
        }

        /// <summary>
        /// 全部取消按钮点击事件
        /// </summary>
        private void CancelAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel viewModel)
            {
                var result = MessageBox.Show("确定要取消所有下载任务吗？", "确认",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    viewModel.CancelAllDownloadsCommand.Execute(null);
                    UpdateEmptyState();
                }
            }
        }
    }
}
