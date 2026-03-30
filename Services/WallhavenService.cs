using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using WallpaperClient.Models;

namespace WallpaperClient.Services
{
    /// <summary>
    /// Wallhaven API 服务实现
    /// </summary>
    public class WallhavenService : IWallhavenService
    {
        private readonly HttpClient _httpClient;
        private string? _apiKey;
        private DateTime? _lastRequestTime;
        private const string BaseUrl = "https://wallhaven.cc/api/v1";

        // 未认证用户：45次/分钟，认证用户：更多请求
        private const int UnauthenticatedRequestInterval = 1334; // ~45次/分钟
        private const int AuthenticatedRequestInterval = 500; // 认证用户可以更快

        public WallhavenService()
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
                Timeout = TimeSpan.FromSeconds(30)
            };

            // 添加必要的请求头，防止被CDN拒绝
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Referer", "https://wallhaven.cc/");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,zh-CN;q=0.8,zh;q=0.7");
        }

        public WallhavenService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <inheritdoc/>
        public DateTime? LastRequestTime => _lastRequestTime;

        /// <inheritdoc/>
        public int RequestInterval => string.IsNullOrEmpty(_apiKey)
            ? UnauthenticatedRequestInterval
            : AuthenticatedRequestInterval;

        /// <inheritdoc/>
        public void SetApiKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _apiKey = null;
                _httpClient.DefaultRequestHeaders.Remove("X-API-Key");
            }
            else
            {
                _apiKey = apiKey;
                _httpClient.DefaultRequestHeaders.Remove("X-API-Key");
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            }
            Log.Information("API Key已更新");
        }

        /// <inheritdoc/>
        public string? GetApiKey()
        {
            return _apiKey;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateApiKeyAsync()
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return false;
            }

            try
            {
                // 尝试获取用户设置来验证API Key
                var response = await GetAsync($"{BaseUrl}/settings");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "验证API Key失败");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<SearchResponse> SearchWallpapersAsync(SearchParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var queryString = parameters.ToQueryString();
            var url = $"{BaseUrl}/search?{queryString}";

            Log.Information("搜索壁纸: {Url}", url);

            return await GetAsync<SearchResponse>(url);
        }

        /// <inheritdoc/>
        public async Task<SearchResponse> SearchWallpapersAsync(string query, int page = 1)
        {
            var parameters = new SearchParameters
            {
                Query = query,
                Page = page
            };

            return await SearchWallpapersAsync(parameters);
        }

        /// <inheritdoc/>
        public async Task<WallpaperData?> GetWallpaperDetailsAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("壁纸ID不能为空", nameof(id));
            }

            try
            {
                var url = $"{BaseUrl}/w/{id}";
                Log.Information("获取壁纸详情: {Id}", id);

                var response = await GetAsync<WallpaperDetailsResponse>(url);
                return response?.Data;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取壁纸详情失败: {Id}", id);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<CollectionResponse> GetCollectionsAsync(string? username = null)
        {
            var url = string.IsNullOrEmpty(username)
                ? $"{BaseUrl}/collections"
                : $"{BaseUrl}/collections/{username}";

            Log.Information("获取收藏集列表: {Url}", url);

            return await GetAsync<CollectionResponse>(url);
        }

        /// <inheritdoc/>
        public async Task<SearchResponse> GetCollectionWallpapersAsync(string username, int collectionId, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("用户名不能为空", nameof(username));
            }

            var url = $"{BaseUrl}/collections/{username}/{collectionId}?page={page}";
            Log.Information("获取收藏集壁纸: {Url}", url);

            return await GetAsync<SearchResponse>(url);
        }

        /// <inheritdoc/>
        public async Task<SearchResponse> GetHotWallpapersAsync(int page = 1)
        {
            var parameters = new SearchParameters
            {
                Sorting = SortingOption.Toplist,
                Page = page
            };

            return await SearchWallpapersAsync(parameters);
        }

        /// <inheritdoc/>
        public async Task<SearchResponse> GetLatestWallpapersAsync(int page = 1)
        {
            var parameters = new SearchParameters
            {
                Sorting = SortingOption.DateAdded,
                Order = SortOrder.Descending,
                Page = page
            };

            return await SearchWallpapersAsync(parameters);
        }

        /// <inheritdoc/>
        public async Task<SearchResponse> GetRandomWallpapersAsync(int count = 24)
        {
            var parameters = new SearchParameters
            {
                Sorting = SortingOption.Random,
                Page = 1
            };

            return await SearchWallpapersAsync(parameters);
        }

        /// <inheritdoc/>
        public async Task<SearchResponse> GetToplistWallpapersAsync(string timeRange = "1M", int page = 1)
        {
            var parameters = new SearchParameters
            {
                Sorting = SortingOption.Toplist,
                Page = page
            };

            // Toplist 支持 1d, 3d, 1w, 1M, 3M, 6M, 1y
            // 注意：需要在API中实现 dateRange 参数

            return await SearchWallpapersAsync(parameters);
        }

        /// <inheritdoc/>
        public async Task<TagSearchResponse> SearchTagsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("搜索关键词不能为空", nameof(query));
            }

            var url = $"{BaseUrl}/tag/{Uri.EscapeDataString(query)}";
            Log.Information("搜索标签: {Query}", query);

            return await GetAsync<TagSearchResponse>(url);
        }

        /// <inheritdoc/>
        public async Task<TagData?> GetTagDetailsAsync(int tagId)
        {
            if (tagId <= 0)
            {
                throw new ArgumentException("标签ID无效", nameof(tagId));
            }

            try
            {
                var url = $"{BaseUrl}/tag/{tagId}";
                var response = await GetAsync<TagDetailsResponse>(url);
                return response?.Data;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取标签详情失败: {TagId}", tagId);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<SearchResponse> SearchByColorAsync(string color, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                throw new ArgumentException("颜色不能为空", nameof(color));
            }

            // 移除 # 前缀
            color = color.TrimStart('#');

            var parameters = new SearchParameters
            {
                Color = color,
                Page = page
            };

            return await SearchWallpapersAsync(parameters);
        }

        /// <inheritdoc/>
        public async Task<SearchResponse> GetSimilarWallpapersAsync(string wallpaperId)
        {
            if (string.IsNullOrWhiteSpace(wallpaperId))
            {
                throw new ArgumentException("壁纸ID不能为空", nameof(wallpaperId));
            }

            var url = $"{BaseUrl}/w/{wallpaperId}/similar";
            Log.Information("获取相似壁纸: {WallpaperId}", wallpaperId);

            return await GetAsync<SearchResponse>(url);
        }

        /// <inheritdoc/>
        public Wallpaper ConvertToWallpaper(WallpaperData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var wallpaper = new Wallpaper
            {
                Id = data.Id,
                Url = data.Path, // 使用原图URL而不是页面URL
                ThumbnailUrl = data.Thumbs?.Large ?? data.Thumbs?.Small ?? string.Empty,
                SmallUrl = data.Thumbs?.Small ?? string.Empty,
                Resolution = data.Resolution,
                Width = data.Width,
                Height = data.Height,
                FileSize = data.FileSize,
                FileType = data.FileType,
                Colors = data.Colors ?? new List<string>(),
                Views = data.Views,
                Favorites = data.Favorites,
                Downloads = data.Downloads,
                UploadedAt = data.CreatedAt,
                IsFavorite = false,
                Source = WallpaperSource.Wallhaven,
                Category = ConvertCategory(data.Category),
                Purity = ConvertPurity(data.Purity),
                Tags = ConvertTags(data.Tags),
                Uploader = data.Uploader?.Username ?? string.Empty,
                UploaderAvatar = data.Uploader?.Avatar?.Px200
                    ?? data.Uploader?.Avatar?.Px128
                    ?? data.Uploader?.Avatar?.Px32
                    ?? data.Uploader?.Avatar?.Px20
            };

            return wallpaper;
        }

        /// <inheritdoc/>
        public Collection ConvertToCollection(CollectionData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var collection = new Collection
            {
                Id = data.Id,
                Name = data.Label,
                Count = data.Count,
                IsPublic = data.Public,
                CreatedAt = data.CreatedAt,
                UpdatedAt = data.UpdatedAt ?? DateTime.Now,
                SourceType = CollectionSource.Wallhaven,
                RemoteId = data.Id
            };

            if (data.Wallpapers != null)
            {
                collection.Wallpapers = data.Wallpapers
                    .Select(w => ConvertToWallpaper(w))
                    .ToList();
            }

            return collection;
        }

        /// <inheritdoc/>
        public async Task<string?> GetDownloadUrlAsync(string wallpaperId)
        {
            var details = await GetWallpaperDetailsAsync(wallpaperId);
            return details?.Path;
        }

        /// <inheritdoc/>
        public bool CanMakeRequest()
        {
            if (!_lastRequestTime.HasValue)
            {
                return true;
            }

            var elapsed = (DateTime.Now - _lastRequestTime.Value).TotalMilliseconds;
            return elapsed >= RequestInterval;
        }

        #region 私有方法

        private async Task<HttpResponseMessage> GetAsync(string url)
        {
            // 等待请求间隔
            await WaitForRequestInterval();

            try
            {
                var response = await _httpClient.GetAsync(url);
                _lastRequestTime = DateTime.Now;

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Log.Warning("API请求失败: {StatusCode} - {Content}",
                        response.StatusCode, errorContent);
                }

                return response;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "API请求异常: {Url}", url);
                throw;
            }
        }

        private async Task<T> GetAsync<T>(string url) where T : class
        {
            try
            {
                var response = await GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("API返回错误状态码: {StatusCode}", response.StatusCode);
                    return Activator.CreateInstance<T>();
                }

                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    Log.Warning("API返回空内容");
                    return Activator.CreateInstance<T>();
                }

                var result = JsonConvert.DeserializeObject<T>(content);
                return result ?? Activator.CreateInstance<T>();
            }
            catch (JsonException ex)
            {
                Log.Error(ex, "JSON反序列化失败: {Url}", url);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "请求失败: {Url}", url);
                throw;
            }
        }

        private async Task WaitForRequestInterval()
        {
            if (_lastRequestTime.HasValue)
            {
                var elapsed = (DateTime.Now - _lastRequestTime.Value).TotalMilliseconds;
                var waitTime = RequestInterval - (int)elapsed;

                if (waitTime > 0)
                {
                    await Task.Delay(waitTime);
                }
            }
        }

        private WallpaperCategory ConvertCategory(string? category)
        {
            return category?.ToLowerInvariant() switch
            {
                "anime" => WallpaperCategory.Anime,
                "people" => WallpaperCategory.People,
                _ => WallpaperCategory.General
            };
        }

        private WallpaperPurity ConvertPurity(string? purity)
        {
            return purity?.ToLowerInvariant() switch
            {
                "nsfw" => WallpaperPurity.NSFW,
                "sketchy" => WallpaperPurity.Sketchy,
                _ => WallpaperPurity.SFW
            };
        }

        private List<Tag> ConvertTags(List<TagData> tagDataList)
        {
            if (tagDataList == null || tagDataList.Count == 0)
            {
                return new List<Tag>();
            }

            return tagDataList.Select(t => new Tag
            {
                Id = t.Id,
                Name = t.Name,
                Alias = t.Alias,
                CategoryId = t.CategoryId,
                Category = t.Category,
                Url = t.Url
            }).ToList();
        }

        #endregion

        #region 内部响应类

        private class WallpaperDetailsResponse
        {
            public WallpaperData Data { get; set; } = new();
        }

        private class TagDetailsResponse
        {
            public TagData Data { get; set; } = new();
        }

        #endregion
    }
}
