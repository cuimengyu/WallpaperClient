using System;
using System.Collections.Generic;
using System.Linq;

namespace WallpaperClient.Models
{
    /// <summary>
    /// 搜索参数模型
    /// </summary>
    public class SearchParameters
    {
        /// <summary>
        /// 搜索关键词
        /// </summary>
        public string? Query { get; set; }

        /// <summary>
        /// 标签列表
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// 分类筛选（General/Anime/People）
        /// </summary>
        public WallpaperCategory? Category { get; set; }

        /// <summary>
        /// 纯度筛选（SFW/Sketchy/NSFW）
        /// </summary>
        public WallpaperPurity? Purity { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public SortingOption Sorting { get; set; } = SortingOption.DateAdded;

        /// <summary>
        /// 排序顺序
        /// </summary>
        public SortOrder Order { get; set; } = SortOrder.Descending;

        /// <summary>
        /// 最小分辨率宽度
        /// </summary>
        public int? MinWidth { get; set; }

        /// <summary>
        /// 最小分辨率高度
        /// </summary>
        public int? MinHeight { get; set; }

        /// <summary>
        /// 分辨率比例
        /// </summary>
        public AspectRatio? AspectRatio { get; set; }

        /// <summary>
        /// 颜色筛选（十六进制颜色代码）
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; } = 24;

        /// <summary>
        /// 用户名（搜索特定用户的上传）
        /// </summary>
        public string? Uploader { get; set; }

        /// <summary>
        /// 是否包含AI生成内容
        /// true  = 包含 AI 内容（ai_art_filter=0）
        /// false = 过滤 AI 内容（ai_art_filter=1）
        /// null  = 不附加该参数
        /// </summary>
        public bool? IncludeAI { get; set; }

        /// <summary>
        /// 纯度筛选组合（可多选）
        /// </summary>
        public PurityFilter PurityFilter { get; set; } = PurityFilter.SFW;

        /// <summary>
        /// 分类筛选组合（可多选）
        /// </summary>
        public CategoryFilter CategoryFilter { get; set; } = CategoryFilter.All;

        /// <summary>
        /// 转换为 Wallhaven API 查询字符串
        /// </summary>
        public string ToQueryString()
        {
            var parameters = new List<string>();

            var queryParts = new List<string>();

            if (!string.IsNullOrWhiteSpace(Query))
            {
                queryParts.Add(Query.Trim());
            }

            if (Tags.Count > 0)
            {
                queryParts.AddRange(
                    Tags.Where(t => !string.IsNullOrWhiteSpace(t))
                        .Select(t => t.Trim()));
            }

            if (queryParts.Count > 0)
            {
                parameters.Add($"q={Uri.EscapeDataString(string.Join(" ", queryParts))}");
            }

            parameters.Add($"categories={GetCategoryString()}");
            parameters.Add($"purity={GetPurityString()}");
            parameters.Add($"sorting={GetSortingString()}");
            parameters.Add($"order={GetOrderString()}");

            if (MinWidth.HasValue)
            {
                parameters.Add($"atleast={MinWidth}x{MinHeight ?? 1080}");
            }

            if (AspectRatio.HasValue)
            {
                parameters.Add($"ratios={GetAspectRatioString(AspectRatio.Value)}");
            }

            if (!string.IsNullOrWhiteSpace(Color))
            {
                parameters.Add($"colors={Color.Trim().TrimStart('#')}");
            }

            parameters.Add($"page={Page}");

            if (PageSize > 0)
            {
                parameters.Add($"per_page={PageSize}");
            }

            if (!string.IsNullOrWhiteSpace(Uploader))
            {
                parameters.Add($"q={Uri.EscapeDataString($"@{Uploader.Trim()}")}");
            }

            if (IncludeAI.HasValue)
            {
                parameters.Add($"ai_art_filter={(IncludeAI.Value ? "0" : "1")}");
            }

            return string.Join("&", parameters);
        }

        private string GetCategoryString()
        {
            if (Category.HasValue)
            {
                return Category.Value switch
                {
                    WallpaperCategory.General => "100",
                    WallpaperCategory.Anime => "010",
                    WallpaperCategory.People => "001",
                    _ => "111"
                };
            }

            int general = (CategoryFilter & CategoryFilter.General) != 0 ? 1 : 0;
            int anime = (CategoryFilter & CategoryFilter.Anime) != 0 ? 1 : 0;
            int people = (CategoryFilter & CategoryFilter.People) != 0 ? 1 : 0;
            return $"{general}{anime}{people}";
        }

        private string GetPurityString()
        {
            if (Purity.HasValue)
            {
                return Purity.Value switch
                {
                    WallpaperPurity.SFW => "100",
                    WallpaperPurity.Sketchy => "010",
                    WallpaperPurity.NSFW => "001",
                    _ => "100"
                };
            }

            int sfw = (PurityFilter & PurityFilter.SFW) != 0 ? 1 : 0;
            int sketchy = (PurityFilter & PurityFilter.Sketchy) != 0 ? 1 : 0;
            int nsfw = (PurityFilter & PurityFilter.NSFW) != 0 ? 1 : 0;
            return $"{sfw}{sketchy}{nsfw}";
        }

        private string GetSortingString()
        {
            return Sorting switch
            {
                SortingOption.DateAdded => "date_added",
                SortingOption.Relevance => "relevance",
                SortingOption.Random => "random",
                SortingOption.Views => "views",
                SortingOption.Favorites => "favorites",
                SortingOption.Toplist => "toplist",
                _ => "date_added"
            };
        }

        private string GetOrderString()
        {
            return Order switch
            {
                SortOrder.Ascending => "asc",
                SortOrder.Descending => "desc",
                _ => "desc"
            };
        }

        private static string GetAspectRatioString(global::WallpaperClient.Models.AspectRatio ratio)
        {
            return ratio switch
            {
                global::WallpaperClient.Models.AspectRatio._16x9 => "16x9",
                global::WallpaperClient.Models.AspectRatio._16x10 => "16x10",
                global::WallpaperClient.Models.AspectRatio._21x9 => "21x9",
                global::WallpaperClient.Models.AspectRatio._32x9 => "32x9",
                global::WallpaperClient.Models.AspectRatio._48x9 => "48x9",
                global::WallpaperClient.Models.AspectRatio._4x3 => "4x3",
                global::WallpaperClient.Models.AspectRatio._5x4 => "5x4",
                global::WallpaperClient.Models.AspectRatio._1x1 => "1x1",
                _ => "16x9"
            };
        }
    }

    /// <summary>
    /// 排序选项
    /// </summary>
    public enum SortingOption
    {
        DateAdded,
        Relevance,
        Random,
        Views,
        Favorites,
        Toplist
    }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public enum SortOrder
    {
        Ascending,
        Descending
    }

    /// <summary>
    /// 分辨率比例
    /// </summary>
    public enum AspectRatio
    {
        _16x9,
        _16x10,
        _21x9,
        _32x9,
        _48x9,
        _4x3,
        _5x4,
        _1x1
    }

    /// <summary>
    /// 纯度筛选标志
    /// </summary>
    [Flags]
    public enum PurityFilter
    {
        None = 0,
        SFW = 1,
        Sketchy = 2,
        NSFW = 4,
        All = SFW | Sketchy | NSFW
    }

    /// <summary>
    /// 分类筛选标志
    /// </summary>
    [Flags]
    public enum CategoryFilter
    {
        None = 0,
        General = 1,
        Anime = 2,
        People = 4,
        All = General | Anime | People
    }
}
