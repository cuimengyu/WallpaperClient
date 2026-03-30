using System;
using System.Collections.Generic;

namespace WallpaperClient.Models
{
    /// <summary>
    /// 应用设置模型
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 设置唯一标识
        /// </summary>
        public int Id { get; set; } = 1;

        #region API 设置

        /// <summary>
        /// Wallhaven API Key
        /// </summary>
        public string? WallhavenApiKey { get; set; }

        /// <summary>
        /// Unsplash API Key
        /// </summary>
        public string? UnsplashApiKey { get; set; }

        /// <summary>
        /// Pexels API Key
        /// </summary>
        public string? PexelsApiKey { get; set; }

        #endregion

        #region 下载设置

        /// <summary>
        /// 壁纸保存路径
        /// </summary>
        public string DownloadPath { get; set; } =
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "WallpaperClient");

        /// <summary>
        /// 文件命名规则
        /// </summary>
        public FileNamingRule NamingRule { get; set; } = FileNamingRule.Id_Resolution;

        /// <summary>
        /// 自定义命名格式
        /// </summary>
        public string? CustomNamingFormat { get; set; }

        /// <summary>
        /// 并发下载数
        /// </summary>
        public int MaxConcurrentDownloads { get; set; } = 3;

        /// <summary>
        /// 下载速度限制（KB/s，0表示不限制）
        /// </summary>
        public int DownloadSpeedLimit { get; set; } = 0;

        /// <summary>
        /// 是否启用断点续传
        /// </summary>
        public bool EnableResumeDownload { get; set; } = true;

        /// <summary>
        /// 下载失败重试次数
        /// </summary>
        public int RetryCount { get; set; } = 3;

        #endregion

        #region 壁纸更换设置

        /// <summary>
        /// 是否启用自动更换壁纸
        /// </summary>
        public bool AutoChangeEnabled { get; set; } = false;

        /// <summary>
        /// 自动更换间隔（分钟）
        /// </summary>
        public int AutoChangeInterval { get; set; } = 30;

        /// <summary>
        /// 壁纸更换来源
        /// </summary>
        public WallpaperChangeSource ChangeSource { get; set; } = WallpaperChangeSource.LocalRandom;

        /// <summary>
        /// 壁纸更换模式
        /// </summary>
        public WallpaperChangeMode ChangeMode { get; set; } = WallpaperChangeMode.Random;

        /// <summary>
        /// 更换壁纸时使用的收藏集ID
        /// </summary>
        public int? ChangeCollectionId { get; set; }

        /// <summary>
        /// 更换壁纸时使用的搜索参数ID
        /// </summary>
        public int? ChangeSearchId { get; set; }

        /// <summary>
        /// 壁纸样式
        /// </summary>
        public WallpaperStyle WallpaperStyle { get; set; } = WallpaperStyle.Fill;

        /// <summary>
        /// 是否在多显示器上使用同一壁纸
        /// </summary>
        public bool SameWallpaperOnAllMonitors { get; set; } = false;

        #endregion

        #region 应用设置

        /// <summary>
        /// 是否开机自启
        /// </summary>
        public bool StartWithWindows { get; set; } = false;

        /// <summary>
        /// 是否最小化到系统托盘
        /// </summary>
        public bool MinimizeToTray { get; set; } = true;

        /// <summary>
        /// 关闭时是否最小化到托盘而非退出
        /// </summary>
        public bool CloseToTray { get; set; } = true;

        /// <summary>
        /// 应用主题
        /// </summary>
        public AppTheme Theme { get; set; } = AppTheme.SystemDefault;

        /// <summary>
        /// 应用语言
        /// </summary>
        public AppLanguage Language { get; set; } = AppLanguage.Chinese;

        /// <summary>
        /// 是否显示托盘图标
        /// </summary>
        public bool ShowTrayIcon { get; set; } = true;

        /// <summary>
        /// 壁纸更换时是否显示通知
        /// </summary>
        public bool ShowChangeNotification { get; set; } = true;

        #endregion

        #region 搜索设置

        /// <summary>
        /// 默认搜索参数
        /// </summary>
        public SearchParameters? DefaultSearchParameters { get; set; }

        /// <summary>
        /// 保存的搜索历史数量
        /// </summary>
        public int MaxSearchHistoryCount { get; set; } = 20;

        /// <summary>
        /// 是否保存搜索历史
        /// </summary>
        public bool SaveSearchHistory { get; set; } = true;

        #endregion

        #region 缓存设置

        /// <summary>
        /// 缩略图缓存路径
        /// </summary>
        public string ThumbnailCachePath { get; set; } =
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WallpaperClient", "Cache", "Thumbnails");

        /// <summary>
        /// 最大缓存大小（MB，0表示不限制）
        /// </summary>
        public int MaxCacheSize { get; set; } = 500;

        /// <summary>
        /// 缓存过期天数
        /// </summary>
        public int CacheExpirationDays { get; set; } = 30;

        /// <summary>
        /// 是否自动清理缓存
        /// </summary>
        public bool AutoCleanCache { get; set; } = true;

        #endregion

        #region 显示设置

        /// <summary>
        /// 壁纸列表显示模式
        /// </summary>
        public DisplayMode DisplayMode { get; set; } = DisplayMode.Grid;

        /// <summary>
        /// 网格显示时的列数
        /// </summary>
        public int GridColumns { get; set; } = 4;

        /// <summary>
        /// 是否显示壁纸信息
        /// </summary>
        public bool ShowWallpaperInfo { get; set; } = true;

        /// <summary>
        /// 是否显示壁纸标签
        /// </summary>
        public bool ShowWallpaperTags { get; set; } = false;

        /// <summary>
        /// 缩略图质量（1-100）
        /// </summary>
        public int ThumbnailQuality { get; set; } = 85;

        #endregion

        /// <summary>
        /// 设置创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 设置更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 文件命名规则
    /// </summary>
    public enum FileNamingRule
    {
        /// <summary>
        /// ID_分辨率（如：wallhaven-abc123_1920x1080.jpg）
        /// </summary>
        Id_Resolution,

        /// <summary>
        /// ID（如：wallhaven-abc123.jpg）
        /// </summary>
        Id,

        /// <summary>
        /// 时间戳（如：20240101_120000.jpg）
        /// </summary>
        Timestamp,

        /// <summary>
        /// 随机名称
        /// </summary>
        Random,

        /// <summary>
        /// 自定义格式
        /// </summary>
        Custom
    }

    /// <summary>
    /// 壁纸更换来源
    /// </summary>
    public enum WallpaperChangeSource
    {
        /// <summary>
        /// 本地随机
        /// </summary>
        LocalRandom,

        /// <summary>
        /// 本地顺序
        /// </summary>
        LocalSequential,

        /// <summary>
        /// 收藏集
        /// </summary>
        Collection,

        /// <summary>
        /// 在线最新
        /// </summary>
        OnlineLatest,

        /// <summary>
        /// 在线热门
        /// </summary>
        OnlinePopular,

        /// <summary>
        /// 自定义搜索
        /// </summary>
        CustomSearch
    }

    /// <summary>
    /// 壁纸更换模式
    /// </summary>
    public enum WallpaperChangeMode
    {
        /// <summary>
        /// 随机
        /// </summary>
        Random,

        /// <summary>
        /// 顺序
        /// </summary>
        Sequential,

        /// <summary>
        /// 按评分
        /// </summary>
        ByRating
    }

    /// <summary>
    /// 壁纸样式
    /// </summary>
    public enum WallpaperStyle
    {
        /// <summary>
        /// 填充
        /// </summary>
        Fill,

        /// <summary>
        /// 适应
        /// </summary>
        Fit,

        /// <summary>
        /// 拉伸
        /// </summary>
        Stretch,

        /// <summary>
        /// 平铺
        /// </summary>
        Tile,

        /// <summary>
        /// 居中
        /// </summary>
        Center,

        /// <summary>
        /// 跨区
        /// </summary>
        Span
    }

    /// <summary>
    /// 应用主题
    /// </summary>
    public enum AppTheme
    {
        /// <summary>
        /// 跟随系统
        /// </summary>
        SystemDefault,

        /// <summary>
        /// 浅色
        /// </summary>
        Light,

        /// <summary>
        /// 深色
        /// </summary>
        Dark
    }

    /// <summary>
    /// 应用语言
    /// </summary>
    public enum AppLanguage
    {
        Chinese,
        English
    }

    /// <summary>
    /// 显示模式
    /// </summary>
    public enum DisplayMode
    {
        /// <summary>
        /// 网格
        /// </summary>
        Grid,

        /// <summary>
        /// 列表
        /// </summary>
        List,

        /// <summary>
        /// 瀑布流
        /// </summary>
        Waterfall
    }
}
