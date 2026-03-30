using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Serilog;
using WallpaperClient.Models;
using WallpaperClient.Services;

namespace WallpaperClient.ViewModels
{
    /// <summary>
    /// 主窗口ViewModel
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IWallhavenService? _wallhavenService;
        private readonly IDownloadService? _downloadService;
        private readonly IDatabaseService? _databaseService;
        private readonly IWallpaperService? _wallpaperService;

        #region 私有字段

        private ViewModelBase _currentView = null!;
        private bool _isLoading;
        private string _statusMessage = "就绪";
        private int _currentPage = 1;
        private int _totalPages = 1;
        private string _searchQuery = string.Empty;
        private AppSettings _settings = null!;
        private Wallpaper? _selectedWallpaper;
        private bool _isLocalWallpapersView;
        private bool _isDownloadsView;

        #endregion

        #region 属性

        /// <summary>
        /// 当前显示的视图
        /// </summary>
        public ViewModelBase CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    NextPageCommand.RaiseCanExecuteChanged();
                    PreviousPageCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    NextPageCommand.RaiseCanExecuteChanged();
                    PreviousPageCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                if (SetProperty(ref _totalPages, value))
                {
                    NextPageCommand.RaiseCanExecuteChanged();
                    PreviousPageCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 搜索关键词
        /// </summary>
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        /// <summary>
        /// 应用设置
        /// </summary>
        public AppSettings Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        /// <summary>
        /// 当前选中的壁纸
        /// </summary>
        public Wallpaper? SelectedWallpaper
        {
            get => _selectedWallpaper;
            set => SetProperty(ref _selectedWallpaper, value);
        }

        /// <summary>
        /// 是否处于本地壁纸视图
        /// </summary>
        public bool IsLocalWallpapersView
        {
            get => _isLocalWallpapersView;
            set => SetProperty(ref _isLocalWallpapersView, value);
        }

        /// <summary>
        /// 是否处于下载管理视图
        /// </summary>
        public bool IsDownloadsView
        {
            get => _isDownloadsView;
            set => SetProperty(ref _isDownloadsView, value);
        }

        /// <summary>
        /// 壁纸列表
        /// </summary>
        public ObservableCollection<Wallpaper> Wallpapers { get; set; }

        /// <summary>
        /// 收藏集列表
        /// </summary>
        public ObservableCollection<Collection> Collections { get; set; }

        /// <summary>
        /// 下载任务列表
        /// </summary>
        public ObservableCollection<DownloadTask> DownloadTasks { get; set; }

        /// <summary>
        /// 搜索历史
        /// </summary>
        public ObservableCollection<string> SearchHistory { get; set; }

        #endregion

        #region 命令

        /// <summary>
        /// 搜索命令
        /// </summary>
        public AsyncRelayCommand SearchCommand { get; }

        /// <summary>
        /// 下一页命令
        /// </summary>
        public AsyncRelayCommand NextPageCommand { get; }

        /// <summary>
        /// 上一页命令
        /// </summary>
        public AsyncRelayCommand PreviousPageCommand { get; }

        /// <summary>
        /// 刷新命令
        /// </summary>
        public AsyncRelayCommand RefreshCommand { get; }

        /// <summary>
        /// 下载壁纸命令
        /// </summary>
        public AsyncRelayCommand<Wallpaper> DownloadWallpaperCommand { get; }

        /// <summary>
        /// 设为壁纸命令
        /// </summary>
        public AsyncRelayCommand<Wallpaper> SetAsWallpaperCommand { get; }

        /// <summary>
        /// 收藏壁纸命令
        /// </summary>
        public AsyncRelayCommand<Wallpaper> FavoriteWallpaperCommand { get; }

        /// <summary>
        /// 打开设置命令
        /// </summary>
        public RelayCommand OpenSettingsCommand { get; }

        /// <summary>
        /// 打开本地壁纸命令
        /// </summary>
        public RelayCommand OpenLocalWallpapersCommand { get; }

        /// <summary>
        /// 删除壁纸命令
        /// </summary>
        public AsyncRelayCommand<Wallpaper> DeleteWallpaperCommand { get; }

        /// <summary>
        /// 打开收藏命令
        /// </summary>
        public RelayCommand OpenFavoritesCommand { get; }

        /// <summary>
        /// 打开下载管理命令
        /// </summary>
        public RelayCommand OpenDownloadsCommand { get; }

        /// <summary>
        /// 获取热门壁纸命令
        /// </summary>
        public AsyncRelayCommand GetHotWallpapersCommand { get; }

        /// <summary>
        /// 获取最新壁纸命令
        /// </summary>
        public AsyncRelayCommand GetLatestWallpapersCommand { get; }

        /// <summary>
        /// 获取随机壁纸命令
        /// </summary>
        public AsyncRelayCommand GetRandomWallpapersCommand { get; }

        /// <summary>
        /// 选择壁纸命令
        /// </summary>
        public RelayCommand<Wallpaper> SelectWallpaperCommand { get; }

        /// <summary>
        /// 取消下载命令
        /// </summary>
        public RelayCommand<string> CancelDownloadCommand { get; }

        /// <summary>
        /// 暂停下载命令
        /// </summary>
        public RelayCommand<string> PauseDownloadCommand { get; }

        /// <summary>
        /// 恢复下载命令
        /// </summary>
        public RelayCommand<string> ResumeDownloadCommand { get; }

        /// <summary>
        /// 删除下载任务命令
        /// </summary>
        public RelayCommand<string> RemoveDownloadCommand { get; }

        /// <summary>
        /// 取消所有下载命令
        /// </summary>
        public RelayCommand CancelAllDownloadsCommand { get; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public MainViewModel()
        {
            // 无参构造函数用于设计时
            Wallpapers = new ObservableCollection<Wallpaper>();
            Collections = new ObservableCollection<Collection>();
            DownloadTasks = new ObservableCollection<DownloadTask>();
            SearchHistory = new ObservableCollection<string>();
            _settings = new AppSettings();

            // 初始化命令
            SearchCommand = new AsyncRelayCommand(SearchAsync);
            NextPageCommand = new AsyncRelayCommand(NextPageAsync, CanGoNextPage);
            PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync, CanGoPreviousPage);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            DownloadWallpaperCommand = new AsyncRelayCommand<Wallpaper>(DownloadWallpaperAsync);
            SetAsWallpaperCommand = new AsyncRelayCommand<Wallpaper>(SetAsWallpaperAsync);
            FavoriteWallpaperCommand = new AsyncRelayCommand<Wallpaper>(FavoriteWallpaperAsync);
            DeleteWallpaperCommand = new AsyncRelayCommand<Wallpaper>(DeleteWallpaperAsync);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            OpenLocalWallpapersCommand = new RelayCommand(OpenLocalWallpapers);
            OpenFavoritesCommand = new RelayCommand(OpenFavorites);
            OpenDownloadsCommand = new RelayCommand(OpenDownloads);
            GetHotWallpapersCommand = new AsyncRelayCommand(GetHotWallpapersAsync);
            GetLatestWallpapersCommand = new AsyncRelayCommand(GetLatestWallpapersAsync);
            GetRandomWallpapersCommand = new AsyncRelayCommand(GetRandomWallpapersAsync);
            SelectWallpaperCommand = new RelayCommand<Wallpaper>(SelectWallpaper);
            CancelDownloadCommand = new RelayCommand<string>(CancelDownload);
            PauseDownloadCommand = new RelayCommand<string>(PauseDownload);
            ResumeDownloadCommand = new RelayCommand<string>(ResumeDownload);
            RemoveDownloadCommand = new RelayCommand<string>(RemoveDownload);
            CancelAllDownloadsCommand = new RelayCommand(CancelAllDownloads);
        }

        /// <summary>
        /// 构造函数（依赖注入）
        /// </summary>
        /// <param name="wallhavenService">Wallhaven服务</param>
        /// <param name="downloadService">下载服务</param>
        /// <param name="databaseService">数据库服务</param>
        public MainViewModel(
            IWallhavenService wallhavenService,
            IDownloadService downloadService,
            IDatabaseService databaseService,
            IWallpaperService wallpaperService) : this()
        {
            _wallhavenService = wallhavenService ?? throw new ArgumentNullException(nameof(wallhavenService));
            _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _wallpaperService = wallpaperService ?? throw new ArgumentNullException(nameof(wallpaperService));

            // 注册下载事件
            _downloadService.DownloadCompleted += OnDownloadCompleted;
            _downloadService.DownloadFailed += OnDownloadFailed;
            _downloadService.DownloadProgressChanged += OnDownloadProgressChanged;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 选择壁纸
        /// </summary>
        /// <param name="wallpaper">要选择的壁纸</param>
        public void SelectWallpaper(Wallpaper? wallpaper)
        {
            if (wallpaper != null)
            {
                SelectedWallpaper = wallpaper;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "正在初始化...";

                // 初始化数据库
                if (_databaseService != null)
                {
                    await _databaseService.InitializeAsync();
                    Settings = await _databaseService.GetSettingsAsync();
                }

                // 加载搜索历史
                await LoadSearchHistoryAsync();

                // 加载最新壁纸
                await GetLatestWallpapersAsync();

                StatusMessage = "初始化完成";
            }
            catch (Exception ex)
            {
                StatusMessage = $"初始化失败: {ex.Message}";
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 加载搜索历史
        /// </summary>
        public async Task LoadSearchHistoryAsync()
        {
            if (_databaseService == null) return;

            try
            {
                var history = await _databaseService.GetSearchHistoryAsync(10);
                SearchHistory.Clear();
                foreach (var item in history)
                {
                    SearchHistory.Add(item.Query);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载搜索历史失败: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 搜索壁纸
        /// </summary>
        private async Task SearchAsync()
        {
            if (_wallhavenService == null) return;

            try
            {
                IsLocalWallpapersView = false; // 切换到搜索视图
                IsLoading = true;
                StatusMessage = "正在搜索...";

                var parameters = new SearchParameters
                {
                    Query = SearchQuery,
                    Page = CurrentPage
                };

                var result = await _wallhavenService.SearchWallpapersAsync(parameters);

                Wallpapers.Clear();
                foreach (var item in result.Data)
                {
                    var wallpaper = _wallhavenService.ConvertToWallpaper(item);
                    Wallpapers.Add(wallpaper);
                }

                SelectedWallpaper = Wallpapers.FirstOrDefault();
                TotalPages = result.TotalPages;
                StatusMessage = $"找到 {result.Meta?.Total ?? 0} 张壁纸";

                // 保存搜索历史
                if (!string.IsNullOrWhiteSpace(SearchQuery) && _databaseService != null)
                {
                    await _databaseService.SaveSearchHistoryAsync(SearchQuery);
                    await LoadSearchHistoryAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"搜索失败: {ex.Message}";
                MessageBox.Show($"搜索失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 下一页
        /// </summary>
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await SearchAsync();
            }
        }

        /// <summary>
        /// 上一页
        /// </summary>
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await SearchAsync();
            }
        }

        /// <summary>
        /// 是否可以下一页
        /// </summary>
        private bool CanGoNextPage()
        {
            return CurrentPage < TotalPages && !IsLoading;
        }

        /// <summary>
        /// 是否可以上一页
        /// </summary>
        private bool CanGoPreviousPage()
        {
            return CurrentPage > 1 && !IsLoading;
        }

        /// <summary>
        /// 刷新
        /// </summary>
        private async Task RefreshAsync()
        {
            await SearchAsync();
        }

        /// <summary>
        /// 下载壁纸
        /// </summary>
        private async Task DownloadWallpaperAsync(Wallpaper? wallpaper)
        {
            if (wallpaper == null || _downloadService == null) return;

            try
            {
                StatusMessage = $"正在下载: {wallpaper.Id}";

                // 获取原图URL并更新到wallpaper对象
                if (_wallhavenService != null && string.IsNullOrEmpty(wallpaper.Url))
                {
                    var originalUrl = await _wallhavenService.GetDownloadUrlAsync(wallpaper.Id);
                    if (!string.IsNullOrEmpty(originalUrl))
                    {
                        wallpaper.Url = originalUrl;
                        Log.Information("已获取原图URL: {Id} -> {URL}", wallpaper.Id, originalUrl);
                    }
                }

                var localPath = await _downloadService.DownloadWallpaperAsync(wallpaper);

                if (!string.IsNullOrEmpty(localPath))
                {
                    wallpaper.LocalPath = localPath;
                    wallpaper.DownloadedAt = DateTime.Now;

                    if (_databaseService != null)
                    {
                        await _databaseService.SaveWallpaperAsync(wallpaper);
                    }

                    StatusMessage = $"下载完成: {wallpaper.Id}";
                    MessageBox.Show($"壁纸已保存到: {localPath}", "下载完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"下载失败: {ex.Message}";
                MessageBox.Show($"下载失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 设为壁纸
        /// </summary>
        private async Task SetAsWallpaperAsync(Wallpaper? wallpaper)
        {
            if (wallpaper == null)
            {
                MessageBox.Show("请先选择一张壁纸", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "正在设置壁纸...";

                // 如果壁纸未下载，先下载
                if (string.IsNullOrEmpty(wallpaper.LocalPath))
                {
                    StatusMessage = "正在下载壁纸...";
                    await DownloadWallpaperAsync(wallpaper);
                }

                if (!string.IsNullOrEmpty(wallpaper.LocalPath) && File.Exists(wallpaper.LocalPath))
                {
                    StatusMessage = "正在设置壁纸...";
                    var success = await _wallpaperService!.SetWallpaperAsync(wallpaper.LocalPath, WallpaperStyle.Fill);

                    if (success)
                    {
                        StatusMessage = $"壁纸设置成功: {wallpaper.Id}";
                        MessageBox.Show($"壁纸已成功设置为桌面背景！\n\n分辨率: {wallpaper.Resolution}\n文件: {wallpaper.LocalPath}",
                            "设置成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        StatusMessage = "壁纸设置失败";
                        MessageBox.Show("壁纸设置失败，请检查文件是否存在或是否有权限访问。",
                            "设置失败", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    StatusMessage = "壁纸文件不存在";
                    MessageBox.Show("壁纸文件不存在，无法设置。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"设置壁纸失败: {ex.Message}";
                MessageBox.Show($"设置壁纸失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 收藏壁纸
        /// </summary>
        private async Task FavoriteWallpaperAsync(Wallpaper? wallpaper)
        {
            if (wallpaper == null || _databaseService == null) return;

            try
            {
                wallpaper.IsFavorite = !wallpaper.IsFavorite;
                await _databaseService.UpdateWallpaperAsync(wallpaper);

                StatusMessage = wallpaper.IsFavorite ? $"已收藏: {wallpaper.Id}" : $"已取消收藏: {wallpaper.Id}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"收藏操作失败: {ex.Message}";
                MessageBox.Show($"收藏操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 打开设置
        /// </summary>
        private void OpenSettings()
        {
            IsLocalWallpapersView = false; // 切换到设置视图
            // TODO: 导航到设置页面
            StatusMessage = "设置功能开发中...";
            StatusMessage = "打开设置";
        }

        /// <summary>
        /// 打开本地壁纸
        /// </summary>
        private async void OpenLocalWallpapers()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "正在加载本地壁纸...";

                if (_databaseService == null)
                {
                    MessageBox.Show("数据库服务未初始化", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var localWallpapers = await _databaseService.GetLocalWallpapersAsync(0, 100);

                if (localWallpapers.Count == 0)
                {
                    StatusMessage = "没有本地壁纸";
                    MessageBox.Show("您还没有下载任何壁纸。\n\n请先浏览并下载壁纸，然后在此查看。",
                        "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 切换到本地壁纸视图
                IsLocalWallpapersView = true; // 标记为本地壁纸视图
                IsDownloadsView = false; // 关闭下载管理视图
                Wallpapers.Clear();
                foreach (var wallpaper in localWallpapers)
                {
                    Wallpapers.Add(wallpaper);
                }

                SelectedWallpaper = Wallpapers.FirstOrDefault();
                StatusMessage = $"已加载 {localWallpapers.Count} 张本地壁纸";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载本地壁纸失败: {ex.Message}";
                MessageBox.Show($"加载本地壁纸失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 打开收藏
        /// </summary>
        private async void OpenFavorites()
        {
            try
            {
                IsLocalWallpapersView = false; // 切换到收藏视图
                IsLoading = true;
                StatusMessage = "正在加载收藏...";

                if (_databaseService == null)
                {
                    MessageBox.Show("数据库服务未初始化", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var favoriteWallpapers = await _databaseService.GetFavoriteWallpapersAsync(0, 100);

                if (favoriteWallpapers.Count == 0)
                {
                    StatusMessage = "没有收藏壁纸";
                    MessageBox.Show("您还没有收藏任何壁纸。\n\n浏览壁纸时点击收藏按钮即可添加收藏。",
                        "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 切换到收藏视图
                IsLocalWallpapersView = false; // 关闭本地壁纸视图
                IsDownloadsView = false; // 关闭下载管理视图
                Wallpapers.Clear();
                foreach (var wallpaper in favoriteWallpapers)
                {
                    Wallpapers.Add(wallpaper);
                }

                SelectedWallpaper = Wallpapers.FirstOrDefault();
                StatusMessage = $"已加载 {favoriteWallpapers.Count} 张收藏壁纸";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载收藏壁纸失败: {ex.Message}";
                MessageBox.Show($"加载收藏壁纸失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 打开下载管理
        /// </summary>
        private void OpenDownloads()
        {
            try
            {
                IsLocalWallpapersView = false;
                IsDownloadsView = true;
                StatusMessage = $"下载管理 - 共 {DownloadTasks.Count} 个任务";
            }
            catch (Exception ex)
            {
                StatusMessage = $"打开下载管理失败: {ex.Message}";
                Log.Error(ex, "打开下载管理失败");
            }
        }

        /// <summary>
        /// 删除壁纸
        /// </summary>
        private async Task DeleteWallpaperAsync(Wallpaper? wallpaper)
        {
            if (wallpaper == null)
            {
                MessageBox.Show("请先选择一张壁纸", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 确认删除
                var result = MessageBox.Show(
                    $"确定要删除这张壁纸吗？\n\n" +
                    $"ID: {wallpaper.Id}\n" +
                    $"分辨率: {wallpaper.Resolution}\n\n" +
                    $"此操作将从数据库中删除记录，并删除本地文件（如果存在）。",
                    "确认删除",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                StatusMessage = $"正在删除壁纸: {wallpaper.Id}";

                // 删除本地文件
                if (!string.IsNullOrEmpty(wallpaper.LocalPath) && File.Exists(wallpaper.LocalPath))
                {
                    try
                    {
                        File.Delete(wallpaper.LocalPath);
                        Log.Information("已删除本地文件: {LocalPath}", wallpaper.LocalPath);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "删除本地文件失败: {LocalPath}", wallpaper.LocalPath);
                        MessageBox.Show($"删除本地文件失败: {ex.Message}\n\n将仅从数据库中删除记录。",
                            "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                // 从数据库删除记录
                if (_databaseService != null)
                {
                    var success = await _databaseService.DeleteWallpaperAsync(wallpaper.Id);
                    if (success)
                    {
                        // 从列表中移除
                        Wallpapers.Remove(wallpaper);

                        // 如果删除的是当前选中的壁纸，清除选中状态
                        if (SelectedWallpaper == wallpaper)
                        {
                            SelectedWallpaper = Wallpapers.FirstOrDefault();
                        }

                        StatusMessage = $"壁纸已删除: {wallpaper.Id}";
                        Log.Information("壁纸已从数据库删除: {Id}", wallpaper.Id);
                    }
                    else
                    {
                        MessageBox.Show("从数据库删除记录失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        StatusMessage = "删除失败";
                    }
                }
                else
                {
                    MessageBox.Show("数据库服务未初始化", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusMessage = "删除失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"删除失败: {ex.Message}";
                MessageBox.Show($"删除壁纸失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Error(ex, "删除壁纸失败: {Id}", wallpaper.Id);
            }
        }

        /// <summary>
        /// 获取热门壁纸
        /// </summary>
        private async Task GetHotWallpapersAsync()
        {
            if (_wallhavenService == null) return;

            try
            {
                IsLocalWallpapersView = false; // 切换到热门壁纸视图
                IsLoading = true;
                StatusMessage = "正在加载热门壁纸...";

                var result = await _wallhavenService.GetHotWallpapersAsync(1);

                Wallpapers.Clear();
                foreach (var item in result.Data)
                {
                    var wallpaper = _wallhavenService.ConvertToWallpaper(item);
                    Wallpapers.Add(wallpaper);
                }

                SelectedWallpaper = Wallpapers.FirstOrDefault();
                TotalPages = result.TotalPages;
                StatusMessage = $"加载完成，共 {result.Data.Count} 张壁纸";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载失败: {ex.Message}";
                MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 获取最新壁纸
        /// </summary>
        private async Task GetLatestWallpapersAsync()
        {
            if (_wallhavenService == null) return;

            try
            {
                IsLocalWallpapersView = false; // 切换到最新壁纸视图
                IsLoading = true;
                StatusMessage = "正在加载最新壁纸...";

                var result = await _wallhavenService.GetLatestWallpapersAsync(1);

                Wallpapers.Clear();
                foreach (var item in result.Data)
                {
                    var wallpaper = _wallhavenService.ConvertToWallpaper(item);
                    Wallpapers.Add(wallpaper);
                }

                SelectedWallpaper = Wallpapers.FirstOrDefault();
                TotalPages = result.TotalPages;
                StatusMessage = $"加载完成，共 {result.Data.Count} 张壁纸";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载失败: {ex.Message}";
                MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 获取随机壁纸
        /// </summary>
        private async Task GetRandomWallpapersAsync()
        {
            if (_wallhavenService == null) return;

            try
            {
                IsLocalWallpapersView = false; // 切换到随机壁纸视图
                IsLoading = true;
                StatusMessage = "正在加载随机壁纸...";

                var result = await _wallhavenService.GetRandomWallpapersAsync(24);

                Wallpapers.Clear();
                foreach (var item in result.Data)
                {
                    var wallpaper = _wallhavenService.ConvertToWallpaper(item);
                    Wallpapers.Add(wallpaper);
                }

                SelectedWallpaper = Wallpapers.FirstOrDefault();
                TotalPages = result.TotalPages;
                StatusMessage = $"加载完成，共 {result.Data.Count} 张壁纸";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载失败: {ex.Message}";
                MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 取消下载任务
        /// </summary>
        /// <param name="downloadId">下载任务ID</param>
        private void CancelDownload(string? downloadId)
        {
            if (string.IsNullOrEmpty(downloadId) || _downloadService == null)
            {
                return;
            }

            try
            {
                var success = _downloadService.CancelDownload(downloadId);
                if (success)
                {
                    StatusMessage = "已取消下载任务";
                }
                else
                {
                    StatusMessage = "取消下载任务失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"取消下载失败: {ex.Message}";
                Log.Error(ex, "取消下载失败: {DownloadId}", downloadId);
            }
        }

        /// <summary>
        /// 暂停下载任务
        /// </summary>
        /// <param name="downloadId">下载任务ID</param>
        private void PauseDownload(string? downloadId)
        {
            if (string.IsNullOrEmpty(downloadId) || _downloadService == null)
            {
                return;
            }

            try
            {
                var success = _downloadService.PauseDownload(downloadId);
                if (success)
                {
                    StatusMessage = "已暂停下载任务";
                }
                else
                {
                    StatusMessage = "暂停下载任务失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"暂停下载失败: {ex.Message}";
                Log.Error(ex, "暂停下载失败: {DownloadId}", downloadId);
            }
        }

        /// <summary>
        /// 恢复下载任务
        /// </summary>
        /// <param name="downloadId">下载任务ID</param>
        private void ResumeDownload(string? downloadId)
        {
            if (string.IsNullOrEmpty(downloadId) || _downloadService == null)
            {
                return;
            }

            try
            {
                var success = _downloadService.ResumeDownload(downloadId);
                if (success)
                {
                    StatusMessage = "已恢复下载任务";
                }
                else
                {
                    StatusMessage = "恢复下载任务失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"恢复下载失败: {ex.Message}";
                Log.Error(ex, "恢复下载失败: {DownloadId}", downloadId);
            }
        }

        /// <summary>
        /// 删除下载任务
        /// </summary>
        /// <param name="downloadId">下载任务ID</param>
        private void RemoveDownload(string? downloadId)
        {
            if (string.IsNullOrEmpty(downloadId))
            {
                return;
            }

            try
            {
                var task = DownloadTasks.FirstOrDefault(d => d.Id == downloadId);
                if (task != null)
                {
                    DownloadTasks.Remove(task);
                    StatusMessage = "已删除下载任务";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"删除下载任务失败: {ex.Message}";
                Log.Error(ex, "删除下载任务失败: {DownloadId}", downloadId);
            }
        }

        /// <summary>
        /// 取消所有下载任务
        /// </summary>
        private void CancelAllDownloads()
        {
            if (_downloadService == null)
            {
                return;
            }

            try
            {
                _downloadService.CancelAllDownloads();
                StatusMessage = "已取消所有下载任务";
            }
            catch (Exception ex)
            {
                StatusMessage = $"取消所有下载失败: {ex.Message}";
                Log.Error(ex, "取消所有下载失败");
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 下载完成事件处理
        /// </summary>
        private void OnDownloadCompleted(object? sender, DownloadCompletedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = $"下载完成: {e.WallpaperId}";
            });
        }

        /// <summary>
        /// 下载失败事件处理
        /// </summary>
        private void OnDownloadFailed(object? sender, DownloadFailedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = $"下载失败: {e.WallpaperId} - {e.ErrorMessage}";
            });
        }

        /// <summary>
        /// 下载进度更新事件处理
        /// </summary>
        private void OnDownloadProgressChanged(object? sender, DownloadProgressEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = $"下载中: {e.Progress.WallpaperId} - {e.Progress.ProgressText} ({e.Progress.SpeedText})";
            });
        }

        #endregion
    }
}
