using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WallpaperClient.Models
{
    /// <summary>
    /// Wallhaven API 搜索结果响应
    /// </summary>
    public class SearchResponse
    {
        /// <summary>
        /// 壁纸列表
        /// </summary>
        [JsonProperty("data")]
        public List<WallpaperData> Data { get; set; } = new();

        /// <summary>
        /// 搜索元数据
        /// </summary>
        [JsonProperty("meta")]
        public SearchMeta? Meta { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        [JsonIgnore]
        public int CurrentPage => Meta?.CurrentPage ?? 1;

        /// <summary>
        /// 总页数
        /// </summary>
        [JsonIgnore]
        public int TotalPages => Meta?.LastPage ?? 1;

        /// <summary>
        /// 每页数量
        /// </summary>
        [JsonIgnore]
        public int PerPage => Meta?.PerPage ?? Data.Count;
    }

    /// <summary>
    /// 搜索元数据
    /// </summary>
    public class SearchMeta
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        [JsonProperty("current_page")]
        public int CurrentPage { get; set; }

        /// <summary>
        /// 最后一页页码
        /// </summary>
        [JsonProperty("last_page")]
        public int LastPage { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        [JsonProperty("per_page")]
        public int PerPage { get; set; }

        /// <summary>
        /// 总结果数
        /// </summary>
        [JsonProperty("total")]
        public int Total { get; set; }

        /// <summary>
        /// 搜索查询
        /// </summary>
        [JsonProperty("query")]
        public string? Query { get; set; }

        /// <summary>
        /// 是否有下一页
        /// </summary>
        [JsonIgnore]
        public bool HasNextPage => CurrentPage < LastPage;

        /// <summary>
        /// 是否有上一页
        /// </summary>
        [JsonIgnore]
        public bool HasPreviousPage => CurrentPage > 1;
    }

    /// <summary>
    /// Wallhaven 壁纸数据（API原始格式）
    /// </summary>
    public class WallpaperData
    {
        /// <summary>
        /// 壁纸ID
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 壁纸URL
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 短链接
        /// </summary>
        [JsonProperty("short_url")]
        public string? ShortUrl { get; set; }

        /// <summary>
        /// 缩略图信息
        /// </summary>
        [JsonProperty("thumbs")]
        public ThumbsInfo? Thumbs { get; set; }

        /// <summary>
        /// 原图URL
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 分辨率
        /// </summary>
        [JsonProperty("resolution")]
        public string Resolution { get; set; } = string.Empty;

        /// <summary>
        /// 宽度
        /// </summary>
        [JsonProperty("dimension_x")]
        public int Width { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        [JsonProperty("dimension_y")]
        public int Height { get; set; }

        /// <summary>
        /// 宽高比
        /// </summary>
        [JsonProperty("ratio")]
        public string? Ratio { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        [JsonProperty("file_size")]
        public long FileSize { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        [JsonProperty("file_type")]
        public string FileType { get; set; } = string.Empty;

        /// <summary>
        /// 上传时间
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 主要颜色列表
        /// </summary>
        [JsonProperty("colors")]
        public List<string> Colors { get; set; } = new();

        /// <summary>
        /// 浏览量
        /// </summary>
        [JsonProperty("views")]
        public int Views { get; set; }

        /// <summary>
        /// 收藏数
        /// </summary>
        [JsonProperty("favorites")]
        public int Favorites { get; set; }

        /// <summary>
        /// 下载次数
        /// </summary>
        [JsonProperty("downloads")]
        public int Downloads { get; set; }

        /// <summary>
        /// 壁纸来源
        /// </summary>
        [JsonProperty("source")]
        public string? Source { get; set; }

        /// <summary>
        /// 纯度信息（sfw/sketchy/nsfw）
        /// </summary>
        [JsonProperty("purity")]
        public string Purity { get; set; } = "sfw";

        /// <summary>
        /// 分类信息（general/anime/people）
        /// </summary>
        [JsonProperty("category")]
        public string Category { get; set; } = "general";

        /// <summary>
        /// 标签列表
        /// </summary>
        [JsonProperty("tags")]
        public List<TagData> Tags { get; set; } = new();

        /// <summary>
        /// 上传者信息
        /// </summary>
        [JsonProperty("uploader")]
        public UploaderInfo? Uploader { get; set; }
    }

    /// <summary>
    /// 缩略图信息
    /// </summary>
    public class ThumbsInfo
    {
        [JsonProperty("large")]
        public string? Large { get; set; }

        [JsonProperty("original")]
        public string? Original { get; set; }

        [JsonProperty("small")]
        public string? Small { get; set; }
    }

    /// <summary>
    /// 标签数据
    /// </summary>
    public class TagData
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("alias")]
        public string? Alias { get; set; }

        [JsonProperty("category_id")]
        public int CategoryId { get; set; }

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("purity")]
        public string? Purity { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }
    }

    /// <summary>
    /// 上传者头像信息
    /// </summary>
    public class AvatarInfo
    {
        [JsonProperty("200px")]
        public string? Px200 { get; set; }

        [JsonProperty("128px")]
        public string? Px128 { get; set; }

        [JsonProperty("32px")]
        public string? Px32 { get; set; }

        [JsonProperty("20px")]
        public string? Px20 { get; set; }
    }

    /// <summary>
    /// 上传者信息
    /// </summary>
    public class UploaderInfo
    {
        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("avatar")]
        public AvatarInfo? Avatar { get; set; }

        [JsonProperty("group")]
        public string? Group { get; set; }
    }

    /// <summary>
    /// 收藏集API响应
    /// </summary>
    public class CollectionResponse
    {
        /// <summary>
        /// 收藏集列表
        /// </summary>
        [JsonProperty("data")]
        public List<CollectionData> Data { get; set; } = new();

        /// <summary>
        /// 搜索元数据
        /// </summary>
        [JsonProperty("meta")]
        public SearchMeta? Meta { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        [JsonIgnore]
        public int CurrentPage => Meta?.CurrentPage ?? 1;

        /// <summary>
        /// 总页数
        /// </summary>
        [JsonIgnore]
        public int TotalPages => Meta?.LastPage ?? 1;

        /// <summary>
        /// 每页数量
        /// </summary>
        [JsonIgnore]
        public int PerPage => Meta?.PerPage ?? Data.Count;
    }

    /// <summary>
    /// 收藏集数据（API原始格式）
    /// </summary>
    public class CollectionData
    {
        /// <summary>
        /// 收藏集ID
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// 收藏集标签
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// 收藏集中的壁纸数量
        /// </summary>
        [JsonProperty("count")]
        public int Count { get; set; }

        /// <summary>
        /// 壁纸列表（收藏集详情时使用）
        /// </summary>
        [JsonProperty("wallpapers")]
        public List<WallpaperData>? Wallpapers { get; set; }

        /// <summary>
        /// 是否公开
        /// </summary>
        [JsonProperty("public")]
        public bool Public { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 设置响应
    /// </summary>
    public class SettingsResponse
    {
        [JsonProperty("apikey")]
        public string? ApiKey { get; set; }

        [JsonProperty("max_concurrent_downloads")]
        public int MaxConcurrentDownloads { get; set; }

        [JsonProperty("auto_change_interval")]
        public int AutoChangeInterval { get; set; }

        [JsonProperty("auto_change_enabled")]
        public bool AutoChangeEnabled { get; set; }

        [JsonProperty("download_path")]
        public string DownloadPath { get; set; } = string.Empty;
    }

    /// <summary>
    /// 标签搜索响应
    /// </summary>
    public class TagSearchResponse
    {
        /// <summary>
        /// 标签列表
        /// </summary>
        [JsonProperty("data")]
        public List<TagData> Data { get; set; } = new();

        /// <summary>
        /// 搜索元数据
        /// </summary>
        [JsonProperty("meta")]
        public SearchMeta? Meta { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        [JsonIgnore]
        public int CurrentPage => Meta?.CurrentPage ?? 1;

        /// <summary>
        /// 总页数
        /// </summary>
        [JsonIgnore]
        public int TotalPages => Meta?.LastPage ?? 1;
    }

    /// <summary>
    /// API错误响应
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        [JsonProperty("error")]
        public string? Error { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        [JsonProperty("message")]
        public string? Message { get; set; }

        /// <summary>
        /// HTTP状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 错误详情
        /// </summary>
        [JsonProperty("errors")]
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
