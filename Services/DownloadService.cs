using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using WallpaperClient.Models;

namespace WallpaperClient.Services
{
    /// <summary>
    /// 下载服务实现
    /// </summary>
    public class DownloadService : IDownloadService
    {
        private readonly HttpClient _httpClient;
        private readonly ConcurrentDictionary<string, DownloadTask> _downloadTasks;
        private readonly ConcurrentQueue<DownloadTask> _downloadQueue;
        private readonly SemaphoreSlim _downloadSemaphore;
        private readonly AppSettings _settings;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _isQueueRunning;
        private long _speedLimit;

        public bool IsQueueRunning => _isQueueRunning;

        public event EventHandler<DownloadCompletedEventArgs>? DownloadCompleted;
        public event EventHandler<DownloadProgressEventArgs>? DownloadProgressChanged;
        public event EventHandler<DownloadFailedEventArgs>? DownloadFailed;
        public event EventHandler<DownloadStateChangedEventArgs>? DownloadStateChanged;

        public DownloadService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(30)
            };

            // 添加必要的请求头，防止被CDN拒绝
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Referer", "https://wallhaven.cc/");
            _httpClient.DefaultRequestHeaders.Add("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");

            _downloadTasks = new ConcurrentDictionary<string, DownloadTask>();
            _downloadQueue = new ConcurrentQueue<DownloadTask>();
            _downloadSemaphore = new SemaphoreSlim(3, 3); // 默认并发下载数为3
            _cancellationTokenSource = new CancellationTokenSource();
            _settings = new AppSettings();
            _speedLimit = 0;
        }

        public async Task<string?> DownloadWallpaperAsync(Wallpaper wallpaper, string? savePath = null, IProgress<DownloadProgress>? progress = null)
        {
            if (wallpaper == null)
            {
                throw new ArgumentNullException(nameof(wallpaper));
            }

            // 优先使用最高分辨率原图URL，依次降级到小图和缩略图
            string downloadUrl;
            if (!string.IsNullOrEmpty(wallpaper.Url))
            {
                // 最高分辨率原图（来自 data.Path）
                downloadUrl = wallpaper.Url;
            }
            else if (!string.IsNullOrEmpty(wallpaper.SmallUrl))
            {
                // 小图（来自 data.Thumbs.Small）
                downloadUrl = wallpaper.SmallUrl;
            }
            else if (!string.IsNullOrEmpty(wallpaper.ThumbnailUrl))
            {
                // 缩略图（来自 data.Thumbs.Large）
                downloadUrl = wallpaper.ThumbnailUrl;
            }
            else
            {
                throw new InvalidOperationException("壁纸没有可用的下载URL");
            }

            var fileName = GenerateFileName(wallpaper, _settings.NamingRule, _settings.CustomNamingFormat);
            var finalSavePath = savePath ?? _settings.DownloadPath;

            Log.Information("开始下载壁纸: {Id}, URL: {URL}, 文件名: {FileName}", wallpaper.Id, downloadUrl, fileName);

            return await DownloadFileAsync(downloadUrl, fileName, finalSavePath, progress);
        }

        public async Task<string?> DownloadFileAsync(string url, string fileName, string? savePath = null, IProgress<DownloadProgress>? progress = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL不能为空", nameof(url));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("文件名不能为空", nameof(fileName));
            }

            var finalSavePath = savePath ?? _settings.DownloadPath;

            // 确保目录存在
            if (!Directory.Exists(finalSavePath))
            {
                Directory.CreateDirectory(finalSavePath);
            }

            var filePath = Path.Combine(finalSavePath, fileName);

            // 检查文件是否已存在
            if (File.Exists(filePath))
            {
                Log.Information("文件已存在: {FilePath}", filePath);
                return filePath;
            }

            var downloadId = Guid.NewGuid().ToString();
            var downloadProgress = new DownloadProgress
            {
                DownloadId = downloadId,
                State = DownloadState.Downloading,
                StartTime = DateTime.Now
            };

            try
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                downloadProgress.TotalBytes = totalBytes;

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

                var buffer = new byte[8192];
                var totalBytesRead = 0L;
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var lastReportTime = stopwatch.ElapsedMilliseconds;

                int bytesRead;
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;

                    downloadProgress.BytesDownloaded = totalBytesRead;
                    downloadProgress.Position = totalBytesRead;

                    // 计算速度
                    var elapsed = stopwatch.ElapsedMilliseconds;
                    if (elapsed > 0 && elapsed - lastReportTime >= 100) // 每100ms报告一次
                    {
                        downloadProgress.Speed = (long)(totalBytesRead / (elapsed / 1000.0));
                        lastReportTime = elapsed;
                        progress?.Report(downloadProgress);
                        OnDownloadProgressChanged(downloadProgress);
                    }

                    // 速度限制
                    if (_speedLimit > 0 && downloadProgress.Speed > _speedLimit)
                    {
                        await Task.Delay(10);
                    }
                }

                stopwatch.Stop();

                downloadProgress.State = DownloadState.Completed;
                downloadProgress.BytesDownloaded = totalBytesRead;
                progress?.Report(downloadProgress);

                Log.Information("下载完成: {FilePath}, 大小: {Size} bytes", filePath, totalBytesRead);
                OnDownloadCompleted(downloadId, "", filePath, stopwatch.Elapsed);

                return filePath;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "下载失败: {Url}", url);

                // 删除部分下载的文件
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch
                    {
                        // 忽略删除错误
                    }
                }

                downloadProgress.State = DownloadState.Failed;
                progress?.Report(downloadProgress);
                OnDownloadFailed(downloadId, "", ex.Message, ex);

                return null;
            }
        }

        public Task<List<DownloadResult>> DownloadWallpapersAsync(IEnumerable<Wallpaper> wallpapers, IProgress<BatchDownloadProgress>? progress = null)
        {
            throw new NotImplementedException("批量下载功能尚未实现");
        }

        public string QueueDownload(Wallpaper wallpaper)
        {
            if (wallpaper == null)
            {
                throw new ArgumentNullException(nameof(wallpaper));
            }

            var task = new DownloadTask
            {
                Wallpaper = wallpaper,
                Url = wallpaper.Url,
                SavePath = _settings.DownloadPath,
                FileName = GenerateFileName(wallpaper, _settings.NamingRule, _settings.CustomNamingFormat),
                State = DownloadState.Pending,
                CreatedAt = DateTime.Now
            };

            _downloadQueue.Enqueue(task);
            _downloadTasks.TryAdd(task.Id, task);

            Log.Information("下载任务已加入队列: {WallpaperId}", wallpaper.Id);
            return task.Id;
        }

        public List<string> QueueDownloads(IEnumerable<Wallpaper> wallpapers)
        {
            if (wallpapers == null)
            {
                throw new ArgumentNullException(nameof(wallpapers));
            }

            var ids = new List<string>();
            foreach (var wallpaper in wallpapers)
            {
                ids.Add(QueueDownload(wallpaper));
            }
            return ids;
        }

        public bool CancelDownload(string downloadId)
        {
            if (_downloadTasks.TryGetValue(downloadId, out var task))
            {
                task.State = DownloadState.Cancelled;
                OnDownloadStateChanged(downloadId, DownloadState.Downloading, DownloadState.Cancelled);
                Log.Information("下载任务已取消: {DownloadId}", downloadId);
                return true;
            }
            return false;
        }

        public void CancelAllDownloads()
        {
            foreach (var task in _downloadTasks.Values)
            {
                if (task.State == DownloadState.Downloading || task.State == DownloadState.Pending)
                {
                    task.State = DownloadState.Cancelled;
                    OnDownloadStateChanged(task.Id, task.State, DownloadState.Cancelled);
                }
            }
            Log.Information("所有下载任务已取消");
        }

        public bool PauseDownload(string downloadId)
        {
            if (_downloadTasks.TryGetValue(downloadId, out var task) && task.State == DownloadState.Downloading)
            {
                task.State = DownloadState.Paused;
                OnDownloadStateChanged(downloadId, DownloadState.Downloading, DownloadState.Paused);
                Log.Information("下载任务已暂停: {DownloadId}", downloadId);
                return true;
            }
            return false;
        }

        public bool ResumeDownload(string downloadId)
        {
            if (_downloadTasks.TryGetValue(downloadId, out var task) && task.State == DownloadState.Paused)
            {
                task.State = DownloadState.Pending;
                _downloadQueue.Enqueue(task);
                OnDownloadStateChanged(downloadId, DownloadState.Paused, DownloadState.Pending);
                Log.Information("下载任务已恢复: {DownloadId}", downloadId);
                return true;
            }
            return false;
        }

        public DownloadProgress? GetDownloadProgress(string downloadId)
        {
            if (_downloadTasks.TryGetValue(downloadId, out var task))
            {
                return task.Progress;
            }
            return null;
        }

        public List<DownloadTask> GetAllDownloads()
        {
            return _downloadTasks.Values.ToList();
        }

        public List<DownloadTask> GetActiveDownloads()
        {
            return _downloadTasks.Values.Where(t => t.State == DownloadState.Downloading).ToList();
        }

        public List<DownloadTask> GetPendingDownloads()
        {
            return _downloadTasks.Values.Where(t => t.State == DownloadState.Pending).ToList();
        }

        public List<DownloadTask> GetCompletedDownloads()
        {
            return _downloadTasks.Values.Where(t => t.State == DownloadState.Completed).ToList();
        }

        public void ClearCompletedDownloads()
        {
            var completedIds = _downloadTasks.Values
                .Where(t => t.State == DownloadState.Completed || t.State == DownloadState.Failed || t.State == DownloadState.Cancelled)
                .Select(t => t.Id)
                .ToList();

            foreach (var id in completedIds)
            {
                _downloadTasks.TryRemove(id, out _);
            }

            Log.Information("已清除 {Count} 个已完成的下载任务", completedIds.Count);
        }

        public void SetMaxConcurrentDownloads(int count)
        {
            if (count < 1)
            {
                count = 1;
            }
            else if (count > 10)
            {
                count = 10;
            }

            _downloadSemaphore.Release(count - _downloadSemaphore.CurrentCount);
            _settings.MaxConcurrentDownloads = count;
            Log.Information("并发下载数已设置为: {Count}", count);
        }

        public void SetDownloadSpeedLimit(long bytesPerSecond)
        {
            _speedLimit = bytesPerSecond;
            Log.Information("下载速度限制已设置为: {Speed} bytes/s", bytesPerSecond);
        }

        public bool IsFileExists(string wallpaperId, string? savePath = null)
        {
            var finalSavePath = savePath ?? _settings.DownloadPath;
            var patterns = new[] { $"{wallpaperId}.*", $"wallhaven-{wallpaperId}.*" };

            foreach (var pattern in patterns)
            {
                var files = Directory.GetFiles(finalSavePath, pattern);
                if (files.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public string GetLocalFilePath(Wallpaper wallpaper)
        {
            var fileName = GenerateFileName(wallpaper, _settings.NamingRule, _settings.CustomNamingFormat);
            return Path.Combine(_settings.DownloadPath, fileName);
        }

        public string GenerateFileName(Wallpaper wallpaper, FileNamingRule rule, string? customFormat = null)
        {
            // 优先使用原图URL获取扩展名，然后是小图URL，最后是缩略图URL
            var urlForExtension = wallpaper.Url ?? wallpaper.SmallUrl ?? wallpaper.ThumbnailUrl;
            var extension = GetExtensionFromUrl(urlForExtension) ?? ".jpg";

            return rule switch
            {
                FileNamingRule.Id_Resolution => $"wallhaven-{wallpaper.Id}_{wallpaper.Resolution}{extension}",
                FileNamingRule.Id => $"wallhaven-{wallpaper.Id}{extension}",
                FileNamingRule.Timestamp => $"{DateTime.Now:yyyyMMdd_HHmmss}{extension}",
                FileNamingRule.Random => $"{Guid.NewGuid():N}{extension}",
                FileNamingRule.Custom => !string.IsNullOrEmpty(customFormat)
                    ? $"{customFormat}{extension}"
                    : $"wallhaven-{wallpaper.Id}{extension}",
                _ => $"wallhaven-{wallpaper.Id}{extension}"
            };
        }

        public void StartQueue()
        {
            if (_isQueueRunning)
            {
                return;
            }

            _isQueueRunning = true;
            _ = ProcessQueueAsync();
            Log.Information("下载队列已启动");
        }

        public void StopQueue()
        {
            _isQueueRunning = false;
            Log.Information("下载队列已停止");
        }

        private async Task ProcessQueueAsync()
        {
            while (_isQueueRunning && !_cancellationTokenSource.IsCancellationRequested)
            {
                if (_downloadQueue.TryDequeue(out var task))
                {
                    await _downloadSemaphore.WaitAsync();

                    try
                    {
                        task.State = DownloadState.Downloading;
                        task.StartedAt = DateTime.Now;
                        OnDownloadStateChanged(task.Id, DownloadState.Pending, DownloadState.Downloading);

                        var progress = new Progress<DownloadProgress>(p =>
                        {
                            task.Progress = p;
                            OnDownloadProgressChanged(p);
                        });

                        var result = await DownloadFileAsync(task.Url, task.FileName, task.SavePath, progress);

                        task.State = !string.IsNullOrEmpty(result) ? DownloadState.Completed : DownloadState.Failed;
                        task.CompletedAt = DateTime.Now;

                        if (task.State == DownloadState.Completed && !string.IsNullOrEmpty(result))
                        {
                            task.Wallpaper.LocalPath = result;
                            task.Wallpaper.DownloadedAt = DateTime.Now;
                            OnDownloadCompleted(task.Id, task.Wallpaper.Id, result, task.CompletedAt.Value - task.StartedAt!.Value);
                        }
                        else
                        {
                            OnDownloadFailed(task.Id, task.Wallpaper.Id, "下载失败", null);
                        }

                        OnDownloadStateChanged(task.Id, DownloadState.Downloading, task.State);
                    }
                    finally
                    {
                        _downloadSemaphore.Release();
                    }
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        private string? GetExtensionFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var path = uri.AbsolutePath;
                var extension = Path.GetExtension(path);
                return !string.IsNullOrEmpty(extension) ? extension : ".jpg";
            }
            catch
            {
                return ".jpg";
            }
        }

        protected virtual void OnDownloadCompleted(string downloadId, string wallpaperId, string localPath, TimeSpan duration)
        {
            DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs
            {
                DownloadId = downloadId,
                WallpaperId = wallpaperId,
                LocalPath = localPath,
                Duration = duration
            });
        }

        protected virtual void OnDownloadProgressChanged(DownloadProgress progress)
        {
            DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs
            {
                DownloadId = progress.DownloadId,
                Progress = progress
            });
        }

        protected virtual void OnDownloadFailed(string downloadId, string wallpaperId, string errorMessage, Exception? exception)
        {
            DownloadFailed?.Invoke(this, new DownloadFailedEventArgs
            {
                DownloadId = downloadId,
                WallpaperId = wallpaperId,
                ErrorMessage = errorMessage,
                Exception = exception
            });
        }

        protected virtual void OnDownloadStateChanged(string downloadId, DownloadState oldState, DownloadState newState)
        {
            DownloadStateChanged?.Invoke(this, new DownloadStateChangedEventArgs
            {
                DownloadId = downloadId,
                OldState = oldState,
                NewState = newState
            });
        }
    }
}
