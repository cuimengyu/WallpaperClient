using System;
using System.Collections.Generic;

namespace WallpaperClient.Models
{
    /// <summary>
    /// 壁纸数据模型
    /// </summary>
    public class Wallpaper
    {
        /// <summary>
        /// 壁纸唯一标识
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 壁纸原始URL（最高分辨率原图）
        /// 来自 Wallhaven API 的 data.path 字段
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 缩略图URL（中等分辨率预览图）
        /// 来自 Wallhaven API 的 data.thumbs.large 字段
        /// </summary>
        public string ThumbnailUrl { get; set; } = string.Empty;

        /// <summary>
        /// 小图URL（低分辨率预览图）
        /// 来自 Wallhaven API 的 data.thumbs.small 字段
        /// </summary>
        public string SmallUrl { get; set; } = string.Empty;

        /// <summary>
        /// 本地存储路径
        /// </summary>
        public string? LocalPath { get; set; }

        /// <summary>
        /// 分辨率（如 1920x1080）
        /// </summary>
        public string Resolution { get; set; } = string.Empty;

        /// <summary>
        /// 宽度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 文件类型（jpg, png等）
        /// </summary>
        public string FileType { get; set; } = string.Empty;

        /// <summary>
        /// 主要颜色列表
        /// </summary>
        public List<string> Colors { get; set; } = new();

        /// <summary>
        /// 标签列表
        /// </summary>
        public List<Tag> Tags { get; set; } = new();

        /// <summary>
        /// 分类（General/Anime/People）
        /// </summary>
        public WallpaperCategory Category { get; set; }

        /// <summary>
        /// 纯度（SFW/Sketchy/NSFW）
        /// </summary>
        public WallpaperPurity Purity { get; set; }

        /// <summary>
        /// 浏览量
        /// </summary>
        public int Views { get; set; }

        /// <summary>
        /// 收藏数
        /// </summary>
        public int Favorites { get; set; }

        /// <summary>
        /// 下载次数
        /// </summary>
        public int Downloads { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime UploadedAt { get; set; }

        /// <summary>
        /// 下载时间
        /// </summary>
        public DateTime? DownloadedAt { get; set; }

        /// <summary>
        /// 上传者用户名
        /// </summary>
        public string Uploader { get; set; } = string.Empty;

        /// <summary>
        /// 上传者头像URL
        /// </summary>
        public string? UploaderAvatar { get; set; }

        /// <summary>
        /// 是否已收藏
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// 收藏分组ID
        /// </summary>
        public int? CollectionId { get; set; }

        /// <summary>
        /// 来源平台
        /// </summary>
        public WallpaperSource Source { get; set; }

        /// <summary>
        /// 创建时间（本地记录时间）
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 标签模型
    /// </summary>
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Alias { get; set; }
        public int CategoryId { get; set; }
        public string? Category { get; set; }
        public string? Url { get; set; }
    }

    /// <summary>
    /// 壁纸分类
    /// </summary>
    public enum WallpaperCategory
    {
        General = 0,
        Anime = 1,
        People = 2
    }

    /// <summary>
    /// 壁纸纯度
    /// </summary>
    public enum WallpaperPurity
    {
        SFW = 0,
        Sketchy = 1,
        NSFW = 2
    }

    /// <summary>
    /// 壁纸来源
    /// </summary>
    public enum WallpaperSource
    {
        Wallhaven = 0,
        Unsplash = 1,
        Pexels = 2,
        Local = 3,
        Custom = 4
    }
}
