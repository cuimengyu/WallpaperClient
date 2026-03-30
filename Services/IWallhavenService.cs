using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WallpaperClient.Models;

namespace WallpaperClient.Services
{
    /// <summary>
    /// Wallhaven API 服务接口
    /// </summary>
    public interface IWallhavenService
    {
        /// <summary>
        /// 设置API密钥
        /// </summary>
        /// <param name="apiKey">API密钥</param>
        void SetApiKey(string apiKey);

        /// <summary>
        /// 获取当前API密钥
        /// </summary>
        /// <returns>API密钥</returns>
        string? GetApiKey();

        /// <summary>
        /// 验证API密钥是否有效
        /// </summary>
        /// <returns>是否有效</returns>
        Task<bool> ValidateApiKeyAsync();

        /// <summary>
        /// 搜索壁纸
        /// </summary>
        /// <param name="parameters">搜索参数</param>
        /// <returns>搜索结果</returns>
        Task<SearchResponse> SearchWallpapersAsync(SearchParameters parameters);

        /// <summary>
        /// 搜索壁纸（简化版）
        /// </summary>
        /// <param name="query">搜索关键词</param>
        /// <param name="page">页码</param>
        /// <returns>搜索结果</returns>
        Task<SearchResponse> SearchWallpapersAsync(string query, int page = 1);

        /// <summary>
        /// 获取壁纸详情
        /// </summary>
        /// <param name="id">壁纸ID</param>
        /// <returns>壁纸数据</returns>
        Task<WallpaperData?> GetWallpaperDetailsAsync(string id);

        /// <summary>
        /// 获取用户的收藏集列表
        /// </summary>
        /// <param name="username">用户名（如果使用API Key，可以不传）</param>
        /// <returns>收藏集列表</returns>
        Task<CollectionResponse> GetCollectionsAsync(string? username = null);

        /// <summary>
        /// 获取收藏集中的壁纸
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="collectionId">收藏集ID</param>
        /// <param name="page">页码</param>
        /// <returns>搜索结果（包含壁纸列表）</returns>
        Task<SearchResponse> GetCollectionWallpapersAsync(string username, int collectionId, int page = 1);

        /// <summary>
        /// 获取热门壁纸
        /// </summary>
        /// <param name="page">页码</param>
        /// <returns>搜索结果</returns>
        Task<SearchResponse> GetHotWallpapersAsync(int page = 1);

        /// <summary>
        /// 获取最新壁纸
        /// </summary>
        /// <param name="page">页码</param>
        /// <returns>搜索结果</returns>
        Task<SearchResponse> GetLatestWallpapersAsync(int page = 1);

        /// <summary>
        /// 获取随机壁纸
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>搜索结果</returns>
        Task<SearchResponse> GetRandomWallpapersAsync(int count = 24);

        /// <summary>
        /// 获取排行榜壁纸
        /// </summary>
        /// <param name="timeRange">时间范围（1d, 3d, 1w, 1M, 3M, 6M, 1y）</param>
        /// <param name="page">页码</param>
        /// <returns>搜索结果</returns>
        Task<SearchResponse> GetToplistWallpapersAsync(string timeRange = "1M", int page = 1);

        /// <summary>
        /// 搜索标签
        /// </summary>
        /// <param name="query">搜索关键词</param>
        /// <returns>标签列表</returns>
        Task<TagSearchResponse> SearchTagsAsync(string query);

        /// <summary>
        /// 获取标签详情
        /// </summary>
        /// <param name="tagId">标签ID</param>
        /// <returns>标签数据</returns>
        Task<TagData?> GetTagDetailsAsync(int tagId);

        /// <summary>
        /// 根据颜色搜索壁纸
        /// </summary>
        /// <param name="color">十六进制颜色代码</param>
        /// <param name="page">页码</param>
        /// <returns>搜索结果</returns>
        Task<SearchResponse> SearchByColorAsync(string color, int page = 1);

        /// <summary>
        /// 获取相似壁纸
        /// </summary>
        /// <param name="wallpaperId">壁纸ID</param>
        /// <returns>搜索结果</returns>
        Task<SearchResponse> GetSimilarWallpapersAsync(string wallpaperId);

        /// <summary>
        /// 将API响应的壁纸数据转换为应用模型
        /// </summary>
        /// <param name="data">API壁纸数据</param>
        /// <returns>壁纸模型</returns>
        Wallpaper ConvertToWallpaper(WallpaperData data);

        /// <summary>
        /// 将API响应的收藏集数据转换为应用模型
        /// </summary>
        /// <param name="data">API收藏集数据</param>
        /// <returns>收藏集模型</returns>
        Collection ConvertToCollection(CollectionData data);

        /// <summary>
        /// 获取壁纸原图下载URL
        /// </summary>
        /// <param name="wallpaperId">壁纸ID</param>
        /// <returns>下载URL</returns>
        Task<string?> GetDownloadUrlAsync(string wallpaperId);

        /// <summary>
        /// 获取最后请求时间
        /// </summary>
        DateTime? LastRequestTime { get; }

        /// <summary>
        /// 获取请求间隔（毫秒）
        /// </summary>
        int RequestInterval { get; }

        /// <summary>
        /// 检查是否可以发送请求（遵守API限制）
        /// </summary>
        /// <returns>是否可以发送请求</returns>
        bool CanMakeRequest();
    }
}
