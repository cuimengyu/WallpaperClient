using System;
using System.Threading.Tasks;
using WallpaperClient.Models;

namespace WallpaperClient.Services
{
    /// <summary>
    /// 壁纸服务接口
    /// </summary>
    public interface IWallpaperService
    {
        /// <summary>
        /// 设置桌面壁纸
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        /// <param name="style">壁纸样式</param>
        /// <returns>是否设置成功</returns>
        Task<bool> SetWallpaperAsync(string imagePath, WallpaperStyle style = WallpaperStyle.Fill);

        /// <summary>
        /// 设置桌面壁纸（从 URL 下载）
        /// </summary>
        /// <param name="imageUrl">图片 URL</param>
        /// <param name="style">壁纸样式</param>
        /// <returns>是否设置成功</returns>
        Task<bool> SetWallpaperFromUrlAsync(string imageUrl, WallpaperStyle style = WallpaperStyle.Fill);

        /// <summary>
        /// 获取当前壁纸路径
        /// </summary>
        /// <returns>当前壁纸路径</returns>
        string? GetCurrentWallpaperPath();

        /// <summary>
        /// 获取可用的壁纸样式
        /// </summary>
        /// <returns>壁纸样式列表</returns>
        WallpaperStyle[] GetAvailableStyles();
    }
}
