using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WallpaperClient.Models;

namespace WallpaperClient.Services
{
    /// <summary>
    /// 下载服务接口
    /// </summary>
    public interface IDownloadService
    {
        /// <summary>
        /// 下载壁纸
        /// </summary>
        /// <param name="wallpaper">壁纸对象</param>
        /// <param name="savePath">保存路径（可选，不传则使用默认路径）</param>
        /// <param name="progress">进度回调</param>
        /// <returns>本地文件路径</returns>
        Task<string?> DownloadWallpaperAsync(Wallpaper wallpaper, string? savePath = null, IProgress<DownloadProgress>? progress = null);

        /// <summary>
        /// 下载壁纸（通过URL）
        /// </summary>
        /// <param name="url">壁纸URL</param>
        /// <param name="fileName">文件名</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="progress">进度回调</param>
        /// <returns>本地文件路径</returns>
        Task<string?> DownloadFileAsync(string url, string fileName, string? savePath = null, IProgress<DownloadProgress>? progress = null);

        /// <summary>
        /// 批量下载壁纸
        /// </summary>
        /// <param name="wallpapers">壁纸列表</param>
        /// <param name="progress">进度回调</param>
        /// <returns>下载结果列表</returns>
        Task<List<DownloadResult>> DownloadWallpapersAsync(IEnumerable<Wallpaper> wallpapers, IProgress<BatchDownloadProgress>? progress = null);

        /// <summary>
        /// 将下载任务加入队列
        /// </summary>
        /// <param name="wallpaper">壁纸对象</param>
        /// <returns>下载任务ID</returns>
        string QueueDownload(Wallpaper wallpaper);

        /// <summary>
        /// 批量加入下载队列
        /// </summary>
        /// <param name="wallpapers">壁纸列表</param>
        /// <returns>下载任务ID列表</returns>
        List<string> QueueDownloads(IEnumerable<Wallpaper> wallpapers);

        /// <summary>
        /// 取消下载任务
        /// </summary>
        /// <param name="downloadId">下载任务ID</param>
        /// <returns>是否成功取消</returns>
        bool CancelDownload(string downloadId);

        /// <summary>
        /// 取消所有下载任务
        /// </summary>
        void CancelAllDownloads();

        /// <summary>
        /// 暂停下载任务
        /// </summary>
        /// <param name="downloadId">下载任务ID</param>
        /// <returns>是否成功暂停</returns>
        bool PauseDownload(string downloadId);

        /// <summary>
        /// 恢复下载任务
        /// </summary>
        /// <param name="downloadId">下载任务ID</param>
        /// <returns>是否成功恢复</returns>
        bool ResumeDownload(string downloadId);

        /// <summary>
        /// 获取下载进度
        /// </summary>
        /// <param name="downloadId">下载任务ID</param>
        /// <returns>下载进度信息</returns>
        DownloadProgress? GetDownloadProgress(string downloadId);

        /// <summary>
        /// 获取所有下载任务
        /// </summary>
        /// <returns>下载任务列表</returns>
        List<DownloadTask> GetAllDownloads();

        /// <summary>
        /// 获取正在下载的任务
        /// </summary>
        /// <returns>正在下载的任务列表</returns>
        List<DownloadTask> GetActiveDownloads();

        /// <summary>
        /// 获取等待中的下载任务
        /// </summary>
        /// <returns>等待中的任务列表</returns>
        List<DownloadTask> GetPendingDownloads();

        /// <summary>
        /// 获取已完成的下载任务
        /// </summary>
        /// <returns>已完成的任务列表</returns>
        List<DownloadTask> GetCompletedDownloads();

        /// <summary>
        /// 清除已完成的下载任务
        /// </summary>
        void ClearCompletedDownloads();

        /// <summary>
        /// 设置并发下载数
        /// </summary>
        /// <param name="count">并发数量</param>
        void SetMaxConcurrentDownloads(int count);

        /// <summary>
        /// 设置下载速度限制
        /// </summary>
        /// <param name="bytesPerSecond">每秒字节数（0表示不限制）</param>
        void SetDownloadSpeedLimit(long bytesPerSecond);

        /// <summary>
        /// 检查文件是否已存在
        /// </summary>
        /// <param name="wallpaperId">壁纸ID</param>
        /// <param name="savePath">保存路径</param>
        /// <returns>是否存在</returns>
        bool IsFileExists(string wallpaperId, string? savePath = null);

        /// <summary>
        /// 获取本地文件路径
        /// </summary>
        /// <param name="wallpaper">壁纸对象</param>
        /// <returns>本地文件路径</returns>
        string GetLocalFilePath(Wallpaper wallpaper);

        /// <summary>
        /// 根据命名规则生成文件名
        /// </summary>
        /// <param name="wallpaper">壁纸对象</param>
        /// <param name="rule">命名规则</param>
        /// <param name="customFormat">自定义格式</param>
        /// <returns>文件名</returns>
        string GenerateFileName(Wallpaper wallpaper, FileNamingRule rule, string? customFormat = null);

        /// <summary>
        /// 开始处理下载队列
        /// </summary>
        void StartQueue();

        /// <summary>
        /// 停止处理下载队列
        /// </summary>
        void StopQueue();

        /// <summary>
        /// 下载队列是否正在运行
        /// </summary>
        bool IsQueueRunning { get; }

        /// <summary>
        /// 下载完成事件
        /// </summary>
        event EventHandler<DownloadCompletedEventArgs>? DownloadCompleted;

        /// <summary>
        /// 下载进度更新事件
        /// </summary>
        event EventHandler<DownloadProgressEventArgs>? DownloadProgressChanged;

        /// <summary>
        /// 下载失败事件
        /// </summary>
        event EventHandler<DownloadFailedEventArgs>? DownloadFailed;

        /// <summary>
        /// 下载任务状态改变事件
        /// </summary>
        event EventHandler<DownloadStateChangedEventArgs>? DownloadStateChanged;
    }

    /// <summary>
    /// 下载进度信息
    /// </summary>
    public class DownloadProgress
    {
        /// <summary>
        /// 下载任务ID
        /// </summary>
        public string DownloadId { get; set; } = string.Empty;

        /// <summary>
        /// 壁纸ID
        /// </summary>
        public string WallpaperId { get; set; } = string.Empty;

        /// <summary>
        /// 已下载字节数
        /// </summary>
        public long BytesDownloaded { get; set; }

        /// <summary>
        /// 总字节数
        /// </summary>
        public long TotalBytes { get; set; }

        /// <summary>
        /// 下载百分比（0-100）
        /// </summary>
        public double Progress => TotalBytes > 0 ? (double)BytesDownloaded / TotalBytes * 100 : 0;

        /// <summary>
        /// 下载速度（字节/秒）
        /// </summary>
        public long Speed { get; set; }

        /// <summary>
        /// 剩余时间
        /// </summary>
        public TimeSpan? RemainingTime => Speed > 0 ? TimeSpan.FromSeconds((TotalBytes - BytesDownloaded) / (double)Speed) : null;

        /// <summary>
        /// 下载状态
        /// </summary>
        public DownloadState State { get; set; }

        /// <summary>
        /// 当前下载位置
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        /// 下载开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 格式化的进度文本
        /// </summary>
        public string ProgressText => $"{FormatFileSize(BytesDownloaded)} / {FormatFileSize(TotalBytes)}";

        /// <summary>
        /// 格式化的速度文本
        /// </summary>
        public string SpeedText => $"{FormatFileSize(Speed)}/s";

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// 批量下载进度
    /// </summary>
    public class BatchDownloadProgress
    {
        /// <summary>
        /// 总任务数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 已完成数
        /// </summary>
        public int CompletedCount { get; set; }

        /// <summary>
        /// 失败数
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// 当前下载进度
        /// </summary>
        public DownloadProgress? CurrentProgress { get; set; }

        /// <summary>
        /// 总进度百分比
        /// </summary>
        public double TotalProgress => TotalCount > 0 ? (double)CompletedCount / TotalCount * 100 : 0;
    }

    /// <summary>
    /// 下载结果
    /// </summary>
    public class DownloadResult
    {
        /// <summary>
        /// 壁纸ID
        /// </summary>
        public string WallpaperId { get; set; } = string.Empty;

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 本地文件路径
        /// </summary>
        public string? LocalPath { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 下载耗时
        /// </summary>
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// 下载任务
    /// </summary>
    public class DownloadTask
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 壁纸对象
        /// </summary>
        public Wallpaper Wallpaper { get; set; } = null!;

        /// <summary>
        /// 下载URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 保存路径
        /// </summary>
        public string SavePath { get; set; } = string.Empty;

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 下载状态
        /// </summary>
        public DownloadState State { get; set; } = DownloadState.Pending;

        /// <summary>
        /// 下载进度
        /// </summary>
        public DownloadProgress Progress { get; set; } = new();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// 是否支持断点续传
        /// </summary>
        public bool SupportsResume { get; set; }

        /// <summary>
        /// 优先级（数字越大优先级越高）
        /// </summary>
        public int Priority { get; set; }
    }

    /// <summary>
    /// 下载状态
    /// </summary>
    public enum DownloadState
    {
        /// <summary>
        /// 等待中
        /// </summary>
        Pending,

        /// <summary>
        /// 正在下载
        /// </summary>
        Downloading,

        /// <summary>
        /// 已暂停
        /// </summary>
        Paused,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed,

        /// <summary>
        /// 已失败
        /// </summary>
        Failed,

        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled,

        /// <summary>
        /// 正在重试
        /// </summary>
        Retrying
    }

    /// <summary>
    /// 下载完成事件参数
    /// </summary>
    public class DownloadCompletedEventArgs : EventArgs
    {
        public string DownloadId { get; set; } = string.Empty;
        public string WallpaperId { get; set; } = string.Empty;
        public string LocalPath { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// 下载进度事件参数
    /// </summary>
    public class DownloadProgressEventArgs : EventArgs
    {
        public string DownloadId { get; set; } = string.Empty;
        public DownloadProgress Progress { get; set; } = new();
    }

    /// <summary>
    /// 下载失败事件参数
    /// </summary>
    public class DownloadFailedEventArgs : EventArgs
    {
        public string DownloadId { get; set; } = string.Empty;
        public string WallpaperId { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// 下载状态改变事件参数
    /// </summary>
    public class DownloadStateChangedEventArgs : EventArgs
    {
        public string DownloadId { get; set; } = string.Empty;
        public DownloadState OldState { get; set; }
        public DownloadState NewState { get; set; }
    }
}
