using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WallpaperClient.Models;

namespace WallpaperClient.Services
{
    /// <summary>
    /// 数据库服务接口
    /// </summary>
    public interface IDatabaseService
    {
        #region 初始化

        /// <summary>
        /// 初始化数据库
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// 检查数据库是否已初始化
        /// </summary>
        bool IsInitialized { get; }

        #endregion

        #region 壁纸操作

        /// <summary>
        /// 保存壁纸
        /// </summary>
        /// <param name="wallpaper">壁纸对象</param>
        /// <returns>是否成功</returns>
        Task<bool> SaveWallpaperAsync(Wallpaper wallpaper);

        /// <summary>
        /// 批量保存壁纸
        /// </summary>
        /// <param name="wallpapers">壁纸列表</param>
        /// <returns>成功保存的数量</returns>
        Task<int> SaveWallpapersAsync(IEnumerable<Wallpaper> wallpapers);

        /// <summary>
        /// 更新壁纸
        /// </summary>
        /// <param name="wallpaper">壁纸对象</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateWallpaperAsync(Wallpaper wallpaper);

        /// <summary>
        /// 删除壁纸
        /// </summary>
        /// <param name="wallpaperId">壁纸ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteWallpaperAsync(string wallpaperId);

        /// <summary>
        /// 获取壁纸
        /// </summary>
        /// <param name="wallpaperId">壁纸ID</param>
        /// <returns>壁纸对象</returns>
        Task<Wallpaper?> GetWallpaperAsync(string wallpaperId);

        /// <summary>
        /// 获取所有壁纸
        /// </summary>
        /// <returns>壁纸列表</returns>
        Task<List<Wallpaper>> GetAllWallpapersAsync();

        /// <summary>
        /// 获取壁纸数量
        /// </summary>
        /// <returns>壁纸数量</returns>
        Task<int> GetWallpaperCountAsync();

        /// <summary>
        /// 搜索壁纸
        /// </summary>
        /// <param name="query">搜索关键词</param>
        /// <param name="skip">跳过数量</param>
        /// <param name="take">获取数量</param>
        /// <returns>壁纸列表</returns>
        Task<List<Wallpaper>> SearchWallpapersAsync(string query, int skip = 0, int take = 50);

        /// <summary>
        /// 获取本地壁纸（已下载）
        /// </summary>
        /// <param name="skip">跳过数量</param>
        /// <param name="take">获取数量</param>
        /// <returns>壁纸列表</returns>
        Task<List<Wallpaper>> GetLocalWallpapersAsync(int skip = 0, int take = 50);

        /// <summary>
        /// 获取收藏的壁纸
        /// </summary>
        /// <param name="skip">跳过数量</param>
        /// <param name="take">获取数量</param>
        /// <returns>壁纸列表</returns>
        Task<List<Wallpaper>> GetFavoriteWallpapersAsync(int skip = 0, int take = 50);

        /// <summary>
        /// 设置/取消收藏
        /// </summary>
        /// <param name="wallpaperId">壁纸ID</param>
        /// <param name="isFavorite">是否收藏</param>
        /// <returns>是否成功</returns>
        Task<bool> SetFavoriteAsync(string wallpaperId, bool isFavorite);

        /// <summary>
        /// 根据标签获取壁纸
        /// </summary>
        /// <param name="tag">标签名</param>
        /// <param name="skip">跳过数量</param>
        /// <param name="take">获取数量</param>
        /// <returns>壁纸列表</returns>
        Task<List<Wallpaper>> GetWallpapersByTagAsync(string tag, int skip = 0, int take = 50);

        /// <summary>
        /// 根据分类获取壁纸
        /// </summary>
        /// <param name="category">分类</param>
        /// <param name="skip">跳过数量</param>
        /// <param name="take">获取数量</param>
        /// <returns>壁纸列表</returns>
        Task<List<Wallpaper>> GetWallpapersByCategoryAsync(WallpaperCategory category, int skip = 0, int take = 50);

        /// <summary>
        /// 根据来源获取壁纸
        /// </summary>
        /// <param name="source">来源</param>
        /// <param name="skip">跳过数量</param>
        /// <param name="take">获取数量</param>
        /// <returns>壁纸列表</returns>
        Task<List<Wallpaper>> GetWallpapersBySourceAsync(WallpaperSource source, int skip = 0, int take = 50);

        /// <summary>
        /// 获取随机壁纸
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>壁纸列表</returns>
        Task<List<Wallpaper>> GetRandomWallpapersAsync(int count = 1);

        /// <summary>
        /// 检查壁纸是否存在
        /// </summary>
        /// <param name="wallpaperId">壁纸ID</param>
        /// <returns>是否存在</returns>
        Task<bool> WallpaperExistsAsync(string wallpaperId);

        #endregion

        #region 收藏集操作

        /// <summary>
        /// 保存收藏集
        /// </summary>
        /// <param name="collection">收藏集对象</param>
        /// <returns>是否成功</returns>
        Task<bool> SaveCollectionAsync(Collection collection);

        /// <summary>
        /// 更新收藏集
        /// </summary>
        /// <param name="collection">收藏集对象</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateCollectionAsync(Collection collection);

        /// <summary>
        /// 删除收藏集
        /// </summary>
        /// <param name="collectionId">收藏集ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteCollectionAsync(int collectionId);

        /// <summary>
        /// 获取收藏集
        /// </summary>
        /// <param name="collectionId">收藏集ID</param>
        /// <returns>收藏集对象</returns>
        Task<Collection?> GetCollectionAsync(int collectionId);

        /// <summary>
        /// 获取所有收藏集
        /// </summary>
        /// <returns>收藏集列表</returns>
        Task<List<Collection>> GetAllCollectionsAsync();

        /// <summary>
        /// 获取收藏集数量
        /// </summary>
        /// <returns>收藏集数量</returns>
        Task<int> GetCollectionCountAsync();

        /// <summary>
        /// 添加壁纸到收藏集
        /// </summary>
        /// <param name="collectionId">收藏集ID</param>
        /// <param name="wallpaperId">壁纸ID</param>
        /// <returns>是否成功</returns>
        Task<bool> AddWallpaperToCollectionAsync(int collectionId, string wallpaperId);

        /// <summary>
        /// 从收藏集移除壁纸
        /// </summary>
        /// <param name="collectionId">收藏集ID</param>
        /// <param name="wallpaperId">壁纸ID</param>
        /// <returns>是否成功</returns>
        Task<bool> RemoveWallpaperFromCollectionAsync(int collectionId, string wallpaperId);

        /// <summary>
        /// 获取收藏集中的壁纸
        /// </summary>
        /// <param name="collectionId">收藏集ID</param>
        /// <param name="skip">跳过数量</param>
        /// <param name="take">获取数量</param>
        /// <returns>壁纸列表</returns>
        Task<List<Wallpaper>> GetCollectionWallpapersAsync(int collectionId, int skip = 0, int take = 50);

        #endregion

        #region 标签操作

        /// <summary>
        /// 保存标签
        /// </summary>
        /// <param name="tag">标签对象</param>
        /// <returns>是否成功</returns>
        Task<bool> SaveTagAsync(Tag tag);

        /// <summary>
        /// 批量保存标签
        /// </summary>
        /// <param name="tags">标签列表</param>
        /// <returns>成功保存的数量</returns>
        Task<int> SaveTagsAsync(IEnumerable<Tag> tags);

        /// <summary>
        /// 获取标签
        /// </summary>
        /// <param name="tagId">标签ID</param>
        /// <returns>标签对象</returns>
        Task<Tag?> GetTagAsync(int tagId);

        /// <summary>
        /// 根据名称获取标签
        /// </summary>
        /// <param name="tagName">标签名</param>
        /// <returns>标签对象</returns>
        Task<Tag?> GetTagByNameAsync(string tagName);

        /// <summary>
        /// 获取所有标签
        /// </summary>
        /// <returns>标签列表</returns>
        Task<List<Tag>> GetAllTagsAsync();

        /// <summary>
        /// 搜索标签
        /// </summary>
        /// <param name="query">搜索关键词</param>
        /// <param name="take">获取数量</param>
        /// <returns>标签列表</returns>
        Task<List<Tag>> SearchTagsAsync(string query, int take = 20);

        /// <summary>
        /// 获取热门标签
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>标签列表</returns>
        Task<List<Tag>> GetPopularTagsAsync(int count = 20);

        #endregion

        #region 设置操作

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="settings">设置对象</param>
        /// <returns>是否成功</returns>
        Task<bool> SaveSettingsAsync(AppSettings settings);

        /// <summary>
        /// 获取设置
        /// </summary>
        /// <returns>设置对象</returns>
        Task<AppSettings> GetSettingsAsync();

        /// <summary>
        /// 重置设置为默认值
        /// </summary>
        /// <returns>是否成功</returns>
        Task<bool> ResetSettingsAsync();

        /// <summary>
        /// 更新单个设置项
        /// </summary>
        /// <param name="key">设置键</param>
        /// <param name="value">设置值</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateSettingAsync(string key, object value);

        #endregion

        #region 下载历史操作

        /// <summary>
        /// 保存下载历史
        /// </summary>
        /// <param name="history">下载历史对象</param>
        /// <returns>是否成功</returns>
        Task<bool> SaveDownloadHistoryAsync(DownloadHistory history);

        /// <summary>
        /// 获取下载历史
        /// </summary>
        /// <param name="skip">跳过数量</param>
        /// <param name="take">获取数量</param>
        /// <returns>下载历史列表</returns>
        Task<List<DownloadHistory>> GetDownloadHistoryAsync(int skip = 0, int take = 50);

        /// <summary>
        /// 清除下载历史
        /// </summary>
        /// <returns>是否成功</returns>
        Task<bool> ClearDownloadHistoryAsync();

        /// <summary>
        /// 获取最近的下载历史
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>下载历史列表</returns>
        Task<List<DownloadHistory>> GetRecentDownloadsAsync(int count = 10);

        #endregion

        #region 壁纸更换历史操作

        /// <summary>
        /// 保存壁纸更换历史
        /// </summary>
        /// <param name="history">更换历史对象</param>
        /// <returns>是否成功</returns>
        Task<bool> SaveChangeHistoryAsync(WallpaperChangeHistory history);

        /// <summary>
        /// 获取壁纸更换历史
        /// </summary>
        /// <param name="skip">跳过数量</param>
        /// <param name="take">获取数量</param>
        /// <returns>更换历史列表</returns>
        Task<List<WallpaperChangeHistory>> GetChangeHistoryAsync(int skip = 0, int take = 50);

        /// <summary>
        /// 清除壁纸更换历史
        /// </summary>
        /// <returns>是否成功</returns>
        Task<bool> ClearChangeHistoryAsync();

        /// <summary>
        /// 获取最近的壁纸更换历史
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>更换历史列表</returns>
        Task<List<WallpaperChangeHistory>> GetRecentChangesAsync(int count = 10);

        #endregion

        #region 搜索历史操作

        /// <summary>
        /// 保存搜索历史
        /// </summary>
        /// <param name="query">搜索关键词</param>
        /// <returns>是否成功</returns>
        Task<bool> SaveSearchHistoryAsync(string query);

        /// <summary>
        /// 获取搜索历史
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>搜索历史列表</returns>
        Task<List<SearchHistoryItem>> GetSearchHistoryAsync(int count = 20);

        /// <summary>
        /// 清除搜索历史
        /// </summary>
        /// <returns>是否成功</returns>
        Task<bool> ClearSearchHistoryAsync();

        /// <summary>
        /// 删除单条搜索历史
        /// </summary>
        /// <param name="query">搜索关键词</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteSearchHistoryAsync(string query);

        #endregion

        #region 统计操作

        /// <summary>
        /// 获取壁纸统计信息
        /// </summary>
        /// <returns>统计信息</returns>
        Task<WallpaperStatistics> GetStatisticsAsync();

        /// <summary>
        /// 获取每日下载统计
        /// </summary>
        /// <param name="days">天数</param>
        /// <returns>每日下载数量字典</returns>
        Task<Dictionary<DateTime, int>> GetDailyDownloadStatsAsync(int days = 30);

        /// <summary>
        /// 获取存储使用情况
        /// </summary>
        /// <returns>存储使用字节数</returns>
        Task<long> GetStorageUsageAsync();

        #endregion

        #region 数据维护

        /// <summary>
        /// 清理无效数据
        /// </summary>
        /// <returns>清理的记录数</returns>
        Task<int> CleanupInvalidDataAsync();

        /// <summary>
        /// 优化数据库
        /// </summary>
        /// <returns>是否成功</returns>
        Task<bool> OptimizeDatabaseAsync();

        /// <summary>
        /// 备份数据库
        /// </summary>
        /// <param name="backupPath">备份路径</param>
        /// <returns>是否成功</returns>
        Task<bool> BackupDatabaseAsync(string backupPath);

        /// <summary>
        /// 恢复数据库
        /// </summary>
        /// <param name="backupPath">备份文件路径</param>
        /// <returns>是否成功</returns>
        Task<bool> RestoreDatabaseAsync(string backupPath);

        /// <summary>
        /// 清空所有数据
        /// </summary>
        /// <returns>是否成功</returns>
        Task<bool> ClearAllDataAsync();

        #endregion
    }

    /// <summary>
    /// 下载历史记录
    /// </summary>
    public class DownloadHistory
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 壁纸ID
        /// </summary>
        public string WallpaperId { get; set; } = string.Empty;

        /// <summary>
        /// 壁纸URL
        /// </summary>
        public string WallpaperUrl { get; set; } = string.Empty;

        /// <summary>
        /// 本地路径
        /// </summary>
        public string LocalPath { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 下载时间
        /// </summary>
        public DateTime DownloadedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 下载耗时（毫秒）
        /// </summary>
        public long Duration { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 壁纸更换历史记录
    /// </summary>
    public class WallpaperChangeHistory
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 壁纸ID
        /// </summary>
        public string WallpaperId { get; set; } = string.Empty;

        /// <summary>
        /// 壁纸本地路径
        /// </summary>
        public string? WallpaperPath { get; set; }

        /// <summary>
        /// 更换时间
        /// </summary>
        public DateTime ChangedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更换来源
        /// </summary>
        public WallpaperChangeSource ChangeSource { get; set; }

        /// <summary>
        /// 显示器索引（多显示器情况）
        /// </summary>
        public int MonitorIndex { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Note { get; set; }
    }

    /// <summary>
    /// 搜索历史项
    /// </summary>
    public class SearchHistoryItem
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 搜索关键词
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// 搜索时间
        /// </summary>
        public DateTime SearchedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 搜索结果数量
        /// </summary>
        public int ResultCount { get; set; }
    }

    /// <summary>
    /// 壁纸统计信息
    /// </summary>
    public class WallpaperStatistics
    {
        /// <summary>
        /// 总壁纸数
        /// </summary>
        public int TotalWallpapers { get; set; }

        /// <summary>
        /// 已下载数
        /// </summary>
        public int DownloadedCount { get; set; }

        /// <summary>
        /// 收藏数
        /// </summary>
        public int FavoriteCount { get; set; }

        /// <summary>
        /// 收藏集数
        /// </summary>
        public int CollectionCount { get; set; }

        /// <summary>
        /// 标签数
        /// </summary>
        public int TagCount { get; set; }

        /// <summary>
        /// 总下载次数
        /// </summary>
        public int TotalDownloads { get; set; }

        /// <summary>
        /// 总存储使用（字节）
        /// </summary>
        public long TotalStorageUsed { get; set; }

        /// <summary>
        /// 本月下载数
        /// </summary>
        public int MonthlyDownloads { get; set; }

        /// <summary>
        /// 今日下载数
        /// </summary>
        public int TodayDownloads { get; set; }

        /// <summary>
        /// 各分类统计
        /// </summary>
        public Dictionary<WallpaperCategory, int> CategoryStats { get; set; } = new();

        /// <summary>
        /// 各来源统计
        /// </summary>
        public Dictionary<WallpaperSource, int> SourceStats { get; set; } = new();
    }
}
