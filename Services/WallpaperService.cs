using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Serilog;
using WallpaperClient.Models;

namespace WallpaperClient.Services
{
    /// <summary>
    /// 壁纸服务实现
    /// </summary>
    public class WallpaperService : IWallpaperService
    {
        private readonly HttpClient _httpClient;
        private readonly string _wallpaperCachePath;

        // Windows API 常量
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        // Windows API 函数
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        public WallpaperService()
        {
            // 配置 HttpClientHandler 支持系统代理和自动解压缩
            var handler = new HttpClientHandler
            {
                UseProxy = true,
                Proxy = HttpClient.DefaultProxy, // 使用系统默认代理（支持浏览器代理）
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(60)
            };

            // 添加必要的请求头，防止被CDN拒绝
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Referer", "https://wallhaven.cc/");
            _httpClient.DefaultRequestHeaders.Add("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,zh-CN;q=0.8,zh;q=0.7");

            // 设置壁纸缓存目录
            _wallpaperCachePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WallpaperClient",
                "Wallpapers"
            );

            if (!Directory.Exists(_wallpaperCachePath))
            {
                Directory.CreateDirectory(_wallpaperCachePath);
            }
        }

        /// <inheritdoc/>
        public Task<bool> SetWallpaperAsync(string imagePath, WallpaperStyle style = WallpaperStyle.Fill)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    throw new ArgumentException("图片路径不能为空", nameof(imagePath));
                }

                if (!File.Exists(imagePath))
                {
                    throw new FileNotFoundException("图片文件不存在", imagePath);
                }

                Log.Information("开始设置壁纸: {ImagePath}, 样式: {Style}", imagePath, style);

                // 验证文件格式
                var extension = Path.GetExtension(imagePath).ToLowerInvariant();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".bmp")
                {
                    throw new NotSupportedException($"不支持的图片格式: {extension}");
                }

                // 复制文件到缓存目录（确保文件不会被删除或移动）
                var cachedPath = Path.Combine(_wallpaperCachePath, $"wallpaper_{DateTime.Now:yyyyMMddHHmmss}{extension}");
                File.Copy(imagePath, cachedPath, true);

                Log.Information("壁纸已复制到缓存目录: {CachedPath}", cachedPath);

                // 设置注册表
                SetWallpaperStyle(style);

                // 设置壁纸
                var result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, cachedPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

                if (!result)
                {
                    var error = Marshal.GetLastWin32Error();
                    Log.Error("设置壁纸失败，错误代码: {ErrorCode}", error);
                    return Task.FromResult(false);
                }

                Log.Information("壁纸设置成功");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "设置壁纸失败: {ImagePath}", imagePath);
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SetWallpaperFromUrlAsync(string imageUrl, WallpaperStyle style = WallpaperStyle.Fill)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                throw new ArgumentException("图片 URL 不能为空", nameof(imageUrl));
            }

            try
            {
                Log.Information("从 URL 下载壁纸: {ImageUrl}", imageUrl);

                // 下载图片
                var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);

                // 确定文件扩展名
                var extension = ".jpg";
                if (imageUrl.Contains(".png", StringComparison.OrdinalIgnoreCase))
                {
                    extension = ".png";
                }
                else if (imageUrl.Contains(".bmp", StringComparison.OrdinalIgnoreCase))
                {
                    extension = ".bmp";
                }

                // 保存到临时文件
                var tempPath = Path.Combine(_wallpaperCachePath, $"temp_{DateTime.Now:yyyyMMddHHmmss}{extension}");
                await File.WriteAllBytesAsync(tempPath, imageBytes);

                Log.Information("壁纸已下载到: {TempPath}", tempPath);

                // 设置壁纸
                var result = await SetWallpaperAsync(tempPath, style);

                // 删除临时文件（壁纸已经复制到缓存）
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "从 URL 设置壁纸失败: {ImageUrl}", imageUrl);
                return false;
            }
        }

        /// <inheritdoc/>
        public string? GetCurrentWallpaperPath()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false);
                var wallpaperPath = key?.GetValue("Wallpaper") as string;

                if (string.IsNullOrEmpty(wallpaperPath))
                {
                    // 尝试从系统参数获取
                    var buffer = new StringBuilder(260);
                    IntPtr ptr = Marshal.AllocHGlobal(buffer.Capacity * 2);
                    try
                    {
                        SystemParametersInfo(0x0073, (uint)buffer.Capacity, ptr, 0);
                        wallpaperPath = Marshal.PtrToStringUni(ptr)?.TrimEnd('\0');
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }

                Log.Information("当前壁纸路径: {WallpaperPath}", wallpaperPath);
                return wallpaperPath;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取当前壁纸路径失败");
                return null;
            }
        }

        /// <inheritdoc/>
        public WallpaperStyle[] GetAvailableStyles()
        {
            return Enum.GetValues<WallpaperStyle>();
        }

        /// <summary>
        /// 设置壁纸样式到注册表
        /// </summary>
        /// <param name="style">壁纸样式</param>
        private void SetWallpaperStyle(WallpaperStyle style)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

                if (key == null)
                {
                    Log.Warning("无法打开注册表项: Control Panel\\Desktop");
                    return;
                }

                switch (style)
                {
                    case WallpaperStyle.Center:
                        key.SetValue("WallpaperStyle", "0");
                        key.SetValue("TileWallpaper", "0");
                        break;

                    case WallpaperStyle.Tile:
                        key.SetValue("WallpaperStyle", "0");
                        key.SetValue("TileWallpaper", "1");
                        break;

                    case WallpaperStyle.Stretch:
                        key.SetValue("WallpaperStyle", "2");
                        key.SetValue("TileWallpaper", "0");
                        break;

                    case WallpaperStyle.Fit:
                        key.SetValue("WallpaperStyle", "6");
                        key.SetValue("TileWallpaper", "0");
                        break;

                    case WallpaperStyle.Fill:
                        key.SetValue("WallpaperStyle", "10");
                        key.SetValue("TileWallpaper", "0");
                        break;

                    case WallpaperStyle.Span:
                        key.SetValue("WallpaperStyle", "22");
                        key.SetValue("TileWallpaper", "0");
                        break;

                    default:
                        key.SetValue("WallpaperStyle", "10");
                        key.SetValue("TileWallpaper", "0");
                        break;
                }

                Log.Information("壁纸样式已设置: {Style}", style);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "设置壁纸样式失败");
            }
        }
    }
}
