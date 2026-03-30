using System;
using System.Collections.Generic;

namespace WallpaperClient.Models
{
    /// <summary>
    /// 收藏集模型
    /// </summary>
    public class Collection
    {
        /// <summary>
        /// 收藏集唯一标识
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 收藏集名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 收藏集描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 收藏集中的壁纸数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 封面图片URL
        /// </summary>
        public string? CoverUrl { get; set; }

        /// <summary>
        /// 是否为公开收藏集
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 收藏集中的壁纸列表
        /// </summary>
        public List<Wallpaper> Wallpapers { get; set; } = new();

        /// <summary>
        /// 来源（Wallhaven用户名或本地）
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// 来源类型
        /// </summary>
        public CollectionSource SourceType { get; set; }

        /// <summary>
        /// 远程收藏集ID（如果是Wallhaven收藏集）
        /// </summary>
        public int? RemoteId { get; set; }
    }

    /// <summary>
    /// 收藏集来源类型
    /// </summary>
    public enum CollectionSource
    {
        Local = 0,
        Wallhaven = 1,
        Custom = 2
    }
}
