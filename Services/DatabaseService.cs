using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Serilog;
using WallpaperClient.Models;

namespace WallpaperClient.Services
{
    /// <summary>
    /// 数据库服务实现（SQLite）
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        private readonly string _databasePath;
        private SqliteConnection? _connection;
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;

        public DatabaseService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WallpaperClient",
                "Data"
            );

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _databasePath = Path.Combine(appDataPath, "wallpapers.db");
        }

        public DatabaseService(string databasePath)
        {
            _databasePath = databasePath;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var connectionString = $"Data Source={_databasePath}";
                _connection = new SqliteConnection(connectionString);
                await _connection.OpenAsync();

                await CreateTablesAsync();
                _isInitialized = true;

                Log.Information("数据库初始化完成: {Path}", _databasePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "数据库初始化失败");
                throw;
            }
        }

        private async Task CreateTablesAsync()
        {
            if (_connection == null) return;

            var createTablesSql = @"
                -- 壁纸表
                CREATE TABLE IF NOT EXISTS Wallpapers (
                    Id TEXT PRIMARY KEY,
                    Url TEXT NOT NULL,
                    ThumbnailUrl TEXT,
                    SmallUrl TEXT,
                    LocalPath TEXT,
                    Resolution TEXT,
                    Width INTEGER,
                    Height INTEGER,
                    FileSize INTEGER,
                    FileType TEXT,
                    Colors TEXT,
                    Category INTEGER,
                    Purity INTEGER,
                    Views INTEGER,
                    Favorites INTEGER,
                    Downloads INTEGER,
                    UploadedAt TEXT,
                    DownloadedAt TEXT,
                    Uploader TEXT,
                    UploaderAvatar TEXT,
                    IsFavorite INTEGER,
                    CollectionId INTEGER,
                    Source INTEGER,
                    CreatedAt TEXT,
                    UpdatedAt TEXT
                );

                -- 标签表
                CREATE TABLE IF NOT EXISTS Tags (
                    Id INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Alias TEXT,
                    CategoryId INTEGER,
                    Category TEXT,
                    Url TEXT
                );

                -- 壁纸标签关联表
                CREATE TABLE IF NOT EXISTS WallpaperTags (
                    WallpaperId TEXT,
                    TagId INTEGER,
                    PRIMARY KEY (WallpaperId, TagId),
                    FOREIGN KEY (WallpaperId) REFERENCES Wallpapers(Id) ON DELETE CASCADE,
                    FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE
                );

                -- 收藏集表
                CREATE TABLE IF NOT EXISTS Collections (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    Count INTEGER DEFAULT 0,
                    CoverUrl TEXT,
                    IsPublic INTEGER DEFAULT 0,
                    CreatedAt TEXT,
                    UpdatedAt TEXT,
                    Source TEXT,
                    SourceType INTEGER,
                    RemoteId INTEGER
                );

                -- 收藏集壁纸关联表
                CREATE TABLE IF NOT EXISTS CollectionWallpapers (
                    CollectionId INTEGER,
                    WallpaperId TEXT,
                    AddedAt TEXT,
                    PRIMARY KEY (CollectionId, WallpaperId),
                    FOREIGN KEY (CollectionId) REFERENCES Collections(Id) ON DELETE CASCADE,
                    FOREIGN KEY (WallpaperId) REFERENCES Wallpapers(Id) ON DELETE CASCADE
                );

                -- 设置表
                CREATE TABLE IF NOT EXISTS Settings (
                    Id INTEGER PRIMARY KEY CHECK (Id = 1),
                    WallhavenApiKey TEXT,
                    UnsplashApiKey TEXT,
                    PexelsApiKey TEXT,
                    DownloadPath TEXT,
                    NamingRule INTEGER,
                    CustomNamingFormat TEXT,
                    MaxConcurrentDownloads INTEGER,
                    DownloadSpeedLimit INTEGER,
                    EnableResumeDownload INTEGER,
                    RetryCount INTEGER,
                    AutoChangeEnabled INTEGER,
                    AutoChangeInterval INTEGER,
                    ChangeSource INTEGER,
                    ChangeMode INTEGER,
                    ChangeCollectionId INTEGER,
                    ChangeSearchId INTEGER,
                    WallpaperStyle INTEGER,
                    SameWallpaperOnAllMonitors INTEGER,
                    StartWithWindows INTEGER,
                    MinimizeToTray INTEGER,
                    CloseToTray INTEGER,
                    Theme INTEGER,
                    Language INTEGER,
                    ShowTrayIcon INTEGER,
                    ShowChangeNotification INTEGER,
                    MaxSearchHistoryCount INTEGER,
                    SaveSearchHistory INTEGER,
                    ThumbnailCachePath TEXT,
                    MaxCacheSize INTEGER,
                    CacheExpirationDays INTEGER,
                    AutoCleanCache INTEGER,
                    DisplayMode INTEGER,
                    GridColumns INTEGER,
                    ShowWallpaperInfo INTEGER,
                    ShowWallpaperTags INTEGER,
                    ThumbnailQuality INTEGER,
                    CreatedAt TEXT,
                    UpdatedAt TEXT
                );

                -- 下载历史表
                CREATE TABLE IF NOT EXISTS DownloadHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    WallpaperId TEXT,
                    WallpaperUrl TEXT,
                    LocalPath TEXT,
                    FileSize INTEGER,
                    DownloadedAt TEXT,
                    Duration INTEGER,
                    Success INTEGER,
                    ErrorMessage TEXT
                );

                -- 壁纸更换历史表
                CREATE TABLE IF NOT EXISTS WallpaperChangeHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    WallpaperId TEXT,
                    WallpaperPath TEXT,
                    ChangedAt TEXT,
                    ChangeSource INTEGER,
                    MonitorIndex INTEGER,
                    Note TEXT
                );

                -- 搜索历史表
                CREATE TABLE IF NOT EXISTS SearchHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Query TEXT NOT NULL,
                    SearchedAt TEXT,
                    ResultCount INTEGER
                );

                -- 创建索引
                CREATE INDEX IF NOT EXISTS idx_wallpapers_uploaded ON Wallpapers(UploadedAt);
                CREATE INDEX IF NOT EXISTS idx_wallpapers_favorite ON Wallpapers(IsFavorite);
                CREATE INDEX IF NOT EXISTS idx_wallpapers_category ON Wallpapers(Category);
                CREATE INDEX IF NOT EXISTS idx_wallpapers_source ON Wallpapers(Source);
                CREATE INDEX IF NOT EXISTS idx_tags_name ON Tags(Name);
                CREATE INDEX IF NOT EXISTS idx_collections_name ON Collections(Name);
                CREATE INDEX IF NOT EXISTS idx_download_history_date ON DownloadHistory(DownloadedAt);
                CREATE INDEX IF NOT EXISTS idx_change_history_date ON WallpaperChangeHistory(ChangedAt);
                CREATE INDEX IF NOT EXISTS idx_search_history_date ON SearchHistory(SearchedAt);
            ";

            using var command = _connection.CreateCommand();
            command.CommandText = createTablesSql;
            await command.ExecuteNonQueryAsync();
        }

        #region 壁纸操作

        public async Task<bool> SaveWallpaperAsync(Wallpaper wallpaper)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                var sql = @"
                    INSERT OR REPLACE INTO Wallpapers
                    (Id, Url, ThumbnailUrl, SmallUrl, LocalPath, Resolution, Width, Height, FileSize, FileType,
                     Colors, Category, Purity, Views, Favorites, Downloads, UploadedAt, DownloadedAt,
                     Uploader, UploaderAvatar, IsFavorite, CollectionId, Source, CreatedAt, UpdatedAt)
                    VALUES
                    (@Id, @Url, @ThumbnailUrl, @SmallUrl, @LocalPath, @Resolution, @Width, @Height, @FileSize, @FileType,
                     @Colors, @Category, @Purity, @Views, @Favorites, @Downloads, @UploadedAt, @DownloadedAt,
                     @Uploader, @UploaderAvatar, @IsFavorite, @CollectionId, @Source, @CreatedAt, @UpdatedAt)";

                using var command = _connection.CreateCommand();
                command.CommandText = sql;
                AddWallpaperParameters(command, wallpaper);
                await command.ExecuteNonQueryAsync();

                // 保存标签
                await SaveWallpaperTagsAsync(wallpaper.Id, wallpaper.Tags);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "保存壁纸失败: {Id}", wallpaper.Id);
                return false;
            }
        }

        public async Task<int> SaveWallpapersAsync(IEnumerable<Wallpaper> wallpapers)
        {
            var count = 0;
            foreach (var wallpaper in wallpapers)
            {
                if (await SaveWallpaperAsync(wallpaper))
                {
                    count++;
                }
            }
            return count;
        }

        public async Task<bool> UpdateWallpaperAsync(Wallpaper wallpaper)
        {
            wallpaper.UpdatedAt = DateTime.Now;
            return await SaveWallpaperAsync(wallpaper);
        }

        public async Task<bool> DeleteWallpaperAsync(string wallpaperId)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "DELETE FROM Wallpapers WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", wallpaperId);
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "删除壁纸失败: {Id}", wallpaperId);
                return false;
            }
        }

        public async Task<Wallpaper?> GetWallpaperAsync(string wallpaperId)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "SELECT * FROM Wallpapers WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", wallpaperId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return ReadWallpaper(reader);
                }
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取壁纸失败: {Id}", wallpaperId);
                return null;
            }
        }

        public async Task<List<Wallpaper>> GetAllWallpapersAsync()
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var wallpapers = new List<Wallpaper>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "SELECT * FROM Wallpapers ORDER BY CreatedAt DESC";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    wallpapers.Add(ReadWallpaper(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取所有壁纸失败");
            }
            return wallpapers;
        }

        public async Task<int> GetWallpaperCountAsync()
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Wallpapers";
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<List<Wallpaper>> SearchWallpapersAsync(string query, int skip = 0, int take = 50)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var wallpapers = new List<Wallpaper>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM Wallpapers
                    WHERE Id LIKE @Query OR Uploader LIKE @Query
                    ORDER BY CreatedAt DESC
                    LIMIT @Take OFFSET @Skip";
                command.Parameters.AddWithValue("@Query", $"%{query}%");
                command.Parameters.AddWithValue("@Take", take);
                command.Parameters.AddWithValue("@Skip", skip);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    wallpapers.Add(ReadWallpaper(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "搜索壁纸失败: {Query}", query);
            }
            return wallpapers;
        }

        public async Task<List<Wallpaper>> GetLocalWallpapersAsync(int skip = 0, int take = 50)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var wallpapers = new List<Wallpaper>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM Wallpapers
                    WHERE LocalPath IS NOT NULL AND LocalPath != ''
                    ORDER BY DownloadedAt DESC
                    LIMIT @Take OFFSET @Skip";
                command.Parameters.AddWithValue("@Take", take);
                command.Parameters.AddWithValue("@Skip", skip);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    wallpapers.Add(ReadWallpaper(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取本地壁纸失败");
            }
            return wallpapers;
        }

        public async Task<List<Wallpaper>> GetFavoriteWallpapersAsync(int skip = 0, int take = 50)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var wallpapers = new List<Wallpaper>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM Wallpapers
                    WHERE IsFavorite = 1
                    ORDER BY UpdatedAt DESC
                    LIMIT @Take OFFSET @Skip";
                command.Parameters.AddWithValue("@Take", take);
                command.Parameters.AddWithValue("@Skip", skip);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    wallpapers.Add(ReadWallpaper(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取收藏壁纸失败");
            }
            return wallpapers;
        }

        public async Task<bool> SetFavoriteAsync(string wallpaperId, bool isFavorite)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Wallpapers
                    SET IsFavorite = @IsFavorite, UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";
                command.Parameters.AddWithValue("@IsFavorite", isFavorite ? 1 : 0);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("o"));
                command.Parameters.AddWithValue("@Id", wallpaperId);
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "设置收藏状态失败: {Id}", wallpaperId);
                return false;
            }
        }

        public async Task<List<Wallpaper>> GetWallpapersByTagAsync(string tag, int skip = 0, int take = 50)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var wallpapers = new List<Wallpaper>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT w.* FROM Wallpapers w
                    INNER JOIN WallpaperTags wt ON w.Id = wt.WallpaperId
                    INNER JOIN Tags t ON wt.TagId = t.Id
                    WHERE t.Name = @Tag
                    ORDER BY w.CreatedAt DESC
                    LIMIT @Take OFFSET @Skip";
                command.Parameters.AddWithValue("@Tag", tag);
                command.Parameters.AddWithValue("@Take", take);
                command.Parameters.AddWithValue("@Skip", skip);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    wallpapers.Add(ReadWallpaper(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "按标签获取壁纸失败: {Tag}", tag);
            }
            return wallpapers;
        }

        public async Task<List<Wallpaper>> GetWallpapersByCategoryAsync(WallpaperCategory category, int skip = 0, int take = 50)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var wallpapers = new List<Wallpaper>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM Wallpapers
                    WHERE Category = @Category
                    ORDER BY CreatedAt DESC
                    LIMIT @Take OFFSET @Skip";
                command.Parameters.AddWithValue("@Category", (int)category);
                command.Parameters.AddWithValue("@Take", take);
                command.Parameters.AddWithValue("@Skip", skip);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    wallpapers.Add(ReadWallpaper(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "按分类获取壁纸失败: {Category}", category);
            }
            return wallpapers;
        }

        public async Task<List<Wallpaper>> GetWallpapersBySourceAsync(WallpaperSource source, int skip = 0, int take = 50)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var wallpapers = new List<Wallpaper>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM Wallpapers
                    WHERE Source = @Source
                    ORDER BY CreatedAt DESC
                    LIMIT @Take OFFSET @Skip";
                command.Parameters.AddWithValue("@Source", (int)source);
                command.Parameters.AddWithValue("@Take", take);
                command.Parameters.AddWithValue("@Skip", skip);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    wallpapers.Add(ReadWallpaper(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "按来源获取壁纸失败: {Source}", source);
            }
            return wallpapers;
        }

        public async Task<List<Wallpaper>> GetRandomWallpapersAsync(int count = 1)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var wallpapers = new List<Wallpaper>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM Wallpapers
                    ORDER BY RANDOM()
                    LIMIT @Count";
                command.Parameters.AddWithValue("@Count", count);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    wallpapers.Add(ReadWallpaper(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取随机壁纸失败");
            }
            return wallpapers;
        }

        public async Task<bool> WallpaperExistsAsync(string wallpaperId)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Wallpapers WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", wallpaperId);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        #endregion

        #region 收藏集操作

        public async Task<bool> SaveCollectionAsync(Collection collection)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                var sql = @"
                    INSERT OR REPLACE INTO Collections
                    (Id, Name, Description, Count, CoverUrl, IsPublic, CreatedAt, UpdatedAt, Source, SourceType, RemoteId)
                    VALUES
                    (@Id, @Name, @Description, @Count, @CoverUrl, @IsPublic, @CreatedAt, @UpdatedAt, @Source, @SourceType, @RemoteId)";

                using var command = _connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddWithValue("@Id", collection.Id);
                command.Parameters.AddWithValue("@Name", collection.Name);
                command.Parameters.AddWithValue("@Description", collection.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Count", collection.Count);
                command.Parameters.AddWithValue("@CoverUrl", collection.CoverUrl ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IsPublic", collection.IsPublic ? 1 : 0);
                command.Parameters.AddWithValue("@CreatedAt", collection.CreatedAt.ToString("o"));
                command.Parameters.AddWithValue("@UpdatedAt", collection.UpdatedAt.ToString("o"));
                command.Parameters.AddWithValue("@Source", collection.Source ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@SourceType", (int)collection.SourceType);
                command.Parameters.AddWithValue("@RemoteId", collection.RemoteId ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "保存收藏集失败: {Id}", collection.Id);
                return false;
            }
        }

        public async Task<bool> UpdateCollectionAsync(Collection collection)
        {
            collection.UpdatedAt = DateTime.Now;
            return await SaveCollectionAsync(collection);
        }

        public async Task<bool> DeleteCollectionAsync(int collectionId)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "DELETE FROM Collections WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", collectionId);
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "删除收藏集失败: {Id}", collectionId);
                return false;
            }
        }

        public async Task<Collection?> GetCollectionAsync(int collectionId)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "SELECT * FROM Collections WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", collectionId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return ReadCollection(reader);
                }
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取收藏集失败: {Id}", collectionId);
                return null;
            }
        }

        public async Task<List<Collection>> GetAllCollectionsAsync()
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var collections = new List<Collection>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "SELECT * FROM Collections ORDER BY UpdatedAt DESC";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    collections.Add(ReadCollection(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取所有收藏集失败");
            }
            return collections;
        }

        public async Task<int> GetCollectionCountAsync()
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Collections";
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> AddWallpaperToCollectionAsync(int collectionId, string wallpaperId)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    INSERT OR IGNORE INTO CollectionWallpapers (CollectionId, WallpaperId, AddedAt)
                    VALUES (@CollectionId, @WallpaperId, @AddedAt)";
                command.Parameters.AddWithValue("@CollectionId", collectionId);
                command.Parameters.AddWithValue("@WallpaperId", wallpaperId);
                command.Parameters.AddWithValue("@AddedAt", DateTime.Now.ToString("o"));
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "添加壁纸到收藏集失败");
                return false;
            }
        }

        public async Task<bool> RemoveWallpaperFromCollectionAsync(int collectionId, string wallpaperId)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "DELETE FROM CollectionWallpapers WHERE CollectionId = @CollectionId AND WallpaperId = @WallpaperId";
                command.Parameters.AddWithValue("@CollectionId", collectionId);
                command.Parameters.AddWithValue("@WallpaperId", wallpaperId);
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "从收藏集移除壁纸失败");
                return false;
            }
        }

        public async Task<List<Wallpaper>> GetCollectionWallpapersAsync(int collectionId, int skip = 0, int take = 50)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var wallpapers = new List<Wallpaper>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT w.* FROM Wallpapers w
                    INNER JOIN CollectionWallpapers cw ON w.Id = cw.WallpaperId
                    WHERE cw.CollectionId = @CollectionId
                    ORDER BY cw.AddedAt DESC
                    LIMIT @Take OFFSET @Skip";
                command.Parameters.AddWithValue("@CollectionId", collectionId);
                command.Parameters.AddWithValue("@Take", take);
                command.Parameters.AddWithValue("@Skip", skip);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    wallpapers.Add(ReadWallpaper(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取收藏集壁纸失败: {Id}", collectionId);
            }
            return wallpapers;
        }

        #endregion

        #region 标签操作

        public async Task<bool> SaveTagAsync(Tag tag)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                var sql = @"
                    INSERT OR REPLACE INTO Tags (Id, Name, Alias, CategoryId, Category, Url)
                    VALUES (@Id, @Name, @Alias, @CategoryId, @Category, @Url)";

                using var command = _connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddWithValue("@Id", tag.Id);
                command.Parameters.AddWithValue("@Name", tag.Name);
                command.Parameters.AddWithValue("@Alias", tag.Alias ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CategoryId", tag.CategoryId);
                command.Parameters.AddWithValue("@Category", tag.Category ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Url", tag.Url ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "保存标签失败: {Id}", tag.Id);
                return false;
            }
        }

        public async Task<int> SaveTagsAsync(IEnumerable<Tag> tags)
        {
            var count = 0;
            foreach (var tag in tags)
            {
                if (await SaveTagAsync(tag))
                {
                    count++;
                }
            }
            return count;
        }

        public async Task<Tag?> GetTagAsync(int tagId)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "SELECT * FROM Tags WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", tagId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return ReadTag(reader);
                }
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取标签失败: {Id}", tagId);
                return null;
            }
        }

        public async Task<Tag?> GetTagByNameAsync(string tagName)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "SELECT * FROM Tags WHERE Name = @Name";
                command.Parameters.AddWithValue("@Name", tagName);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return ReadTag(reader);
                }
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取标签失败: {Name}", tagName);
                return null;
            }
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var tags = new List<Tag>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "SELECT * FROM Tags ORDER BY Name";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tags.Add(ReadTag(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取所有标签失败");
            }
            return tags;
        }

        public async Task<List<Tag>> SearchTagsAsync(string query, int take = 20)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var tags = new List<Tag>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM Tags
                    WHERE Name LIKE @Query OR Alias LIKE @Query
                    ORDER BY Name
                    LIMIT @Take";
                command.Parameters.AddWithValue("@Query", $"%{query}%");
                command.Parameters.AddWithValue("@Take", take);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tags.Add(ReadTag(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "搜索标签失败: {Query}", query);
            }
            return tags;
        }

        public async Task<List<Tag>> GetPopularTagsAsync(int count = 20)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var tags = new List<Tag>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT t.*, COUNT(wt.WallpaperId) as UsageCount
                    FROM Tags t
                    LEFT JOIN WallpaperTags wt ON t.Id = wt.TagId
                    GROUP BY t.Id
                    ORDER BY UsageCount DESC
                    LIMIT @Count";
                command.Parameters.AddWithValue("@Count", count);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tags.Add(ReadTag(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取热门标签失败");
            }
            return tags;
        }

        #endregion

        #region 设置操作

        public async Task<bool> SaveSettingsAsync(AppSettings settings)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                var sql = @"
                    INSERT OR REPLACE INTO Settings
                    (Id, WallhavenApiKey, UnsplashApiKey, PexelsApiKey, DownloadPath, NamingRule, CustomNamingFormat,
                     MaxConcurrentDownloads, DownloadSpeedLimit, EnableResumeDownload, RetryCount,
                     AutoChangeEnabled, AutoChangeInterval, ChangeSource, ChangeMode, ChangeCollectionId, ChangeSearchId,
                     WallpaperStyle, SameWallpaperOnAllMonitors, StartWithWindows, MinimizeToTray, CloseToTray,
                     Theme, Language, ShowTrayIcon, ShowChangeNotification, MaxSearchHistoryCount, SaveSearchHistory,
                     ThumbnailCachePath, MaxCacheSize, CacheExpirationDays, AutoCleanCache,
                     DisplayMode, GridColumns, ShowWallpaperInfo, ShowWallpaperTags, ThumbnailQuality,
                     CreatedAt, UpdatedAt)
                    VALUES
                    (1, @WallhavenApiKey, @UnsplashApiKey, @PexelsApiKey, @DownloadPath, @NamingRule, @CustomNamingFormat,
                     @MaxConcurrentDownloads, @DownloadSpeedLimit, @EnableResumeDownload, @RetryCount,
                     @AutoChangeEnabled, @AutoChangeInterval, @ChangeSource, @ChangeMode, @ChangeCollectionId, @ChangeSearchId,
                     @WallpaperStyle, @SameWallpaperOnAllMonitors, @StartWithWindows, @MinimizeToTray, @CloseToTray,
                     @Theme, @Language, @ShowTrayIcon, @ShowChangeNotification, @MaxSearchHistoryCount, @SaveSearchHistory,
                     @ThumbnailCachePath, @MaxCacheSize, @CacheExpirationDays, @AutoCleanCache,
                     @DisplayMode, @GridColumns, @ShowWallpaperInfo, @ShowWallpaperTags, @ThumbnailQuality,
                     @CreatedAt, @UpdatedAt)";

                using var command = _connection.CreateCommand();
                command.CommandText = sql;
                AddSettingsParameters(command, settings);
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "保存设置失败");
                return false;
            }
        }

        public async Task<AppSettings> GetSettingsAsync()
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "SELECT * FROM Settings WHERE Id = 1";

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return ReadSettings(reader);
                }

                // 如果没有设置记录，创建默认设置
                var defaultSettings = new AppSettings();
                await SaveSettingsAsync(defaultSettings);
                return defaultSettings;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取设置失败");
                return new AppSettings();
            }
        }

        public async Task<bool> ResetSettingsAsync()
        {
            return await SaveSettingsAsync(new AppSettings());
        }

        public async Task<bool> UpdateSettingAsync(string key, object value)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = $"UPDATE Settings SET {key} = @Value, UpdatedAt = @UpdatedAt WHERE Id = 1";
                command.Parameters.AddWithValue("@Value", value);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("o"));
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "更新设置失败: {Key}", key);
                return false;
            }
        }

        #endregion

        #region 下载历史操作

        public async Task<bool> SaveDownloadHistoryAsync(DownloadHistory history)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                var sql = @"
                    INSERT INTO DownloadHistory
                    (WallpaperId, WallpaperUrl, LocalPath, FileSize, DownloadedAt, Duration, Success, ErrorMessage)
                    VALUES
                    (@WallpaperId, @WallpaperUrl, @LocalPath, @FileSize, @DownloadedAt, @Duration, @Success, @ErrorMessage)";

                using var command = _connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddWithValue("@WallpaperId", history.WallpaperId);
                command.Parameters.AddWithValue("@WallpaperUrl", history.WallpaperUrl);
                command.Parameters.AddWithValue("@LocalPath", history.LocalPath);
                command.Parameters.AddWithValue("@FileSize", history.FileSize);
                command.Parameters.AddWithValue("@DownloadedAt", history.DownloadedAt.ToString("o"));
                command.Parameters.AddWithValue("@Duration", history.Duration);
                command.Parameters.AddWithValue("@Success", history.Success ? 1 : 0);
                command.Parameters.AddWithValue("@ErrorMessage", history.ErrorMessage ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "保存下载历史失败");
                return false;
            }
        }

        public async Task<List<DownloadHistory>> GetDownloadHistoryAsync(int skip = 0, int take = 50)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var history = new List<DownloadHistory>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM DownloadHistory
                    ORDER BY DownloadedAt DESC
                    LIMIT @Take OFFSET @Skip";
                command.Parameters.AddWithValue("@Take", take);
                command.Parameters.AddWithValue("@Skip", skip);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    history.Add(ReadDownloadHistory(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取下载历史失败");
            }
            return history;
        }

        public async Task<bool> ClearDownloadHistoryAsync()
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "DELETE FROM DownloadHistory";
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "清除下载历史失败");
                return false;
            }
        }

        public async Task<List<DownloadHistory>> GetRecentDownloadsAsync(int count = 10)
        {
            return await GetDownloadHistoryAsync(0, count);
        }

        #endregion

        #region 壁纸更换历史操作

        public async Task<bool> SaveChangeHistoryAsync(WallpaperChangeHistory history)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                var sql = @"
                    INSERT INTO WallpaperChangeHistory
                    (WallpaperId, WallpaperPath, ChangedAt, ChangeSource, MonitorIndex, Note)
                    VALUES
                    (@WallpaperId, @WallpaperPath, @ChangedAt, @ChangeSource, @MonitorIndex, @Note)";

                using var command = _connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddWithValue("@WallpaperId", history.WallpaperId);
                command.Parameters.AddWithValue("@WallpaperPath", history.WallpaperPath ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ChangedAt", history.ChangedAt.ToString("o"));
                command.Parameters.AddWithValue("@ChangeSource", (int)history.ChangeSource);
                command.Parameters.AddWithValue("@MonitorIndex", history.MonitorIndex);
                command.Parameters.AddWithValue("@Note", history.Note ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "保存壁纸更换历史失败");
                return false;
            }
        }

        public async Task<List<WallpaperChangeHistory>> GetChangeHistoryAsync(int skip = 0, int take = 50)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var history = new List<WallpaperChangeHistory>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM WallpaperChangeHistory
                    ORDER BY ChangedAt DESC
                    LIMIT @Take OFFSET @Skip";
                command.Parameters.AddWithValue("@Take", take);
                command.Parameters.AddWithValue("@Skip", skip);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    history.Add(ReadChangeHistory(reader));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取壁纸更换历史失败");
            }
            return history;
        }

        public async Task<bool> ClearChangeHistoryAsync()
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "DELETE FROM WallpaperChangeHistory";
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "清除壁纸更换历史失败");
                return false;
            }
        }

        public async Task<List<WallpaperChangeHistory>> GetRecentChangesAsync(int count = 10)
        {
            return await GetChangeHistoryAsync(0, count);
        }

        #endregion

        #region 搜索历史操作

        public async Task<bool> SaveSearchHistoryAsync(string query)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");
            if (string.IsNullOrWhiteSpace(query)) return false;

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO SearchHistory (Query, SearchedAt, ResultCount)
                    VALUES (@Query, @SearchedAt, 0)";
                command.Parameters.AddWithValue("@Query", query);
                command.Parameters.AddWithValue("@SearchedAt", DateTime.Now.ToString("o"));
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "保存搜索历史失败: {Query}", query);
                return false;
            }
        }

        public async Task<List<SearchHistoryItem>> GetSearchHistoryAsync(int count = 20)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var history = new List<SearchHistoryItem>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT * FROM SearchHistory
                    ORDER BY SearchedAt DESC
                    LIMIT @Count";
                command.Parameters.AddWithValue("@Count", count);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    history.Add(new SearchHistoryItem
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Query = reader.GetString(reader.GetOrdinal("Query")),
                        SearchedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("SearchedAt"))),
                        ResultCount = reader.GetInt32(reader.GetOrdinal("ResultCount"))
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取搜索历史失败");
            }
            return history;
        }

        public async Task<bool> ClearSearchHistoryAsync()
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "DELETE FROM SearchHistory";
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "清除搜索历史失败");
                return false;
            }
        }

        public async Task<bool> DeleteSearchHistoryAsync(string query)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "DELETE FROM SearchHistory WHERE Query = @Query";
                command.Parameters.AddWithValue("@Query", query);
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "删除搜索历史失败: {Query}", query);
                return false;
            }
        }

        #endregion

        #region 统计操作

        public async Task<WallpaperStatistics> GetStatisticsAsync()
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var stats = new WallpaperStatistics();
            try
            {
                using var command = _connection.CreateCommand();

                // 总壁纸数
                command.CommandText = "SELECT COUNT(*) FROM Wallpapers";
                stats.TotalWallpapers = Convert.ToInt32(await command.ExecuteScalarAsync());

                // 已下载数
                command.CommandText = "SELECT COUNT(*) FROM Wallpapers WHERE LocalPath IS NOT NULL AND LocalPath != ''";
                stats.DownloadedCount = Convert.ToInt32(await command.ExecuteScalarAsync());

                // 收藏数
                command.CommandText = "SELECT COUNT(*) FROM Wallpapers WHERE IsFavorite = 1";
                stats.FavoriteCount = Convert.ToInt32(await command.ExecuteScalarAsync());

                // 收藏集数
                command.CommandText = "SELECT COUNT(*) FROM Collections";
                stats.CollectionCount = Convert.ToInt32(await command.ExecuteScalarAsync());

                // 标签数
                command.CommandText = "SELECT COUNT(*) FROM Tags";
                stats.TagCount = Convert.ToInt32(await command.ExecuteScalarAsync());

                // 总下载次数
                command.CommandText = "SELECT COUNT(*) FROM DownloadHistory WHERE Success = 1";
                stats.TotalDownloads = Convert.ToInt32(await command.ExecuteScalarAsync());

                // 今日下载数
                command.CommandText = "SELECT COUNT(*) FROM DownloadHistory WHERE Date(DownloadedAt) = Date('now') AND Success = 1";
                stats.TodayDownloads = Convert.ToInt32(await command.ExecuteScalarAsync());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取统计信息失败");
            }
            return stats;
        }

        public async Task<Dictionary<DateTime, int>> GetDailyDownloadStatsAsync(int days = 30)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var stats = new Dictionary<DateTime, int>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    SELECT DATE(DownloadedAt) as Date, COUNT(*) as Count
                    FROM DownloadHistory
                    WHERE DownloadedAt >= DATE('now', '-' || @Days || ' days')
                    GROUP BY DATE(DownloadedAt)
                    ORDER BY Date";
                command.Parameters.AddWithValue("@Days", days);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var date = DateTime.Parse(reader.GetString(0));
                    var count = reader.GetInt32(1);
                    stats[date] = count;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取每日下载统计失败");
            }
            return stats;
        }

        public async Task<long> GetStorageUsageAsync()
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "SELECT SUM(FileSize) FROM Wallpapers WHERE LocalPath IS NOT NULL AND LocalPath != ''";
                var result = await command.ExecuteScalarAsync();
                return result != DBNull.Value ? Convert.ToInt64(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取存储使用情况失败");
                return 0;
            }
        }

        #endregion

        #region 数据维护

        public async Task<int> CleanupInvalidDataAsync()
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            var count = 0;
            try
            {
                // 删除没有壁纸的标签关联
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM WallpaperTags
                        WHERE WallpaperId NOT IN (SELECT Id FROM Wallpapers)";
                    count += await command.ExecuteNonQueryAsync();
                }

                // 删除没有壁纸的收藏集关联
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM CollectionWallpapers
                        WHERE WallpaperId NOT IN (SELECT Id FROM Wallpapers)";
                    count += await command.ExecuteNonQueryAsync();
                }

                Log.Information("清理了 {Count} 条无效数据", count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "清理无效数据失败");
            }
            return count;
        }

        public async Task<bool> OptimizeDatabaseAsync()
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "VACUUM";
                await command.ExecuteNonQueryAsync();

                Log.Information("数据库优化完成");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "数据库优化失败");
                return false;
            }
        }

        public async Task<bool> BackupDatabaseAsync(string backupPath)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                await _connection.CloseAsync();
                File.Copy(_databasePath, backupPath, true);
                await _connection.OpenAsync();

                Log.Information("数据库备份完成: {Path}", backupPath);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "数据库备份失败");
                return false;
            }
        }

        public async Task<bool> RestoreDatabaseAsync(string backupPath)
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                await _connection.CloseAsync();
                File.Copy(backupPath, _databasePath, true);
                await _connection.OpenAsync();

                Log.Information("数据库恢复完成: {Path}", backupPath);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "数据库恢复失败");
                return false;
            }
        }

        public async Task<bool> ClearAllDataAsync()
        {
            if (_connection == null) throw new InvalidOperationException("数据库未初始化");

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    DELETE FROM WallpaperTags;
                    DELETE FROM CollectionWallpapers;
                    DELETE FROM Wallpapers;
                    DELETE FROM Tags;
                    DELETE FROM Collections;
                    DELETE FROM DownloadHistory;
                    DELETE FROM WallpaperChangeHistory;
                    DELETE FROM SearchHistory;";
                await command.ExecuteNonQueryAsync();

                Log.Information("所有数据已清空");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "清空数据失败");
                return false;
            }
        }

        #endregion

        #region 辅助方法

        private void AddWallpaperParameters(SqliteCommand command, Wallpaper wallpaper)
        {
            command.Parameters.AddWithValue("@Id", wallpaper.Id);
            command.Parameters.AddWithValue("@Url", wallpaper.Url);
            command.Parameters.AddWithValue("@ThumbnailUrl", wallpaper.ThumbnailUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@SmallUrl", wallpaper.SmallUrl ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LocalPath", wallpaper.LocalPath ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Resolution", wallpaper.Resolution);
            command.Parameters.AddWithValue("@Width", wallpaper.Width);
            command.Parameters.AddWithValue("@Height", wallpaper.Height);
            command.Parameters.AddWithValue("@FileSize", wallpaper.FileSize);
            command.Parameters.AddWithValue("@FileType", wallpaper.FileType);
            command.Parameters.AddWithValue("@Colors", string.Join(",", wallpaper.Colors));
            command.Parameters.AddWithValue("@Category", (int)wallpaper.Category);
            command.Parameters.AddWithValue("@Purity", (int)wallpaper.Purity);
            command.Parameters.AddWithValue("@Views", wallpaper.Views);
            command.Parameters.AddWithValue("@Favorites", wallpaper.Favorites);
            command.Parameters.AddWithValue("@Downloads", wallpaper.Downloads);
            command.Parameters.AddWithValue("@UploadedAt", wallpaper.UploadedAt.ToString("o"));
            command.Parameters.AddWithValue("@DownloadedAt", wallpaper.DownloadedAt?.ToString("o") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Uploader", wallpaper.Uploader);
            command.Parameters.AddWithValue("@UploaderAvatar", wallpaper.UploaderAvatar ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsFavorite", wallpaper.IsFavorite ? 1 : 0);
            command.Parameters.AddWithValue("@CollectionId", wallpaper.CollectionId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Source", (int)wallpaper.Source);
            command.Parameters.AddWithValue("@CreatedAt", wallpaper.CreatedAt.ToString("o"));
            command.Parameters.AddWithValue("@UpdatedAt", wallpaper.UpdatedAt.ToString("o"));
        }

        private void AddSettingsParameters(SqliteCommand command, AppSettings settings)
        {
            command.Parameters.AddWithValue("@WallhavenApiKey", settings.WallhavenApiKey ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UnsplashApiKey", settings.UnsplashApiKey ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PexelsApiKey", settings.PexelsApiKey ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DownloadPath", settings.DownloadPath);
            command.Parameters.AddWithValue("@NamingRule", (int)settings.NamingRule);
            command.Parameters.AddWithValue("@CustomNamingFormat", settings.CustomNamingFormat ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@MaxConcurrentDownloads", settings.MaxConcurrentDownloads);
            command.Parameters.AddWithValue("@DownloadSpeedLimit", settings.DownloadSpeedLimit);
            command.Parameters.AddWithValue("@EnableResumeDownload", settings.EnableResumeDownload ? 1 : 0);
            command.Parameters.AddWithValue("@RetryCount", settings.RetryCount);
            command.Parameters.AddWithValue("@AutoChangeEnabled", settings.AutoChangeEnabled ? 1 : 0);
            command.Parameters.AddWithValue("@AutoChangeInterval", settings.AutoChangeInterval);
            command.Parameters.AddWithValue("@ChangeSource", (int)settings.ChangeSource);
            command.Parameters.AddWithValue("@ChangeMode", (int)settings.ChangeMode);
            command.Parameters.AddWithValue("@ChangeCollectionId", settings.ChangeCollectionId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ChangeSearchId", settings.ChangeSearchId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@WallpaperStyle", (int)settings.WallpaperStyle);
            command.Parameters.AddWithValue("@SameWallpaperOnAllMonitors", settings.SameWallpaperOnAllMonitors ? 1 : 0);
            command.Parameters.AddWithValue("@StartWithWindows", settings.StartWithWindows ? 1 : 0);
            command.Parameters.AddWithValue("@MinimizeToTray", settings.MinimizeToTray ? 1 : 0);
            command.Parameters.AddWithValue("@CloseToTray", settings.CloseToTray ? 1 : 0);
            command.Parameters.AddWithValue("@Theme", (int)settings.Theme);
            command.Parameters.AddWithValue("@Language", (int)settings.Language);
            command.Parameters.AddWithValue("@ShowTrayIcon", settings.ShowTrayIcon ? 1 : 0);
            command.Parameters.AddWithValue("@ShowChangeNotification", settings.ShowChangeNotification ? 1 : 0);
            command.Parameters.AddWithValue("@MaxSearchHistoryCount", settings.MaxSearchHistoryCount);
            command.Parameters.AddWithValue("@SaveSearchHistory", settings.SaveSearchHistory ? 1 : 0);
            command.Parameters.AddWithValue("@ThumbnailCachePath", settings.ThumbnailCachePath);
            command.Parameters.AddWithValue("@MaxCacheSize", settings.MaxCacheSize);
            command.Parameters.AddWithValue("@CacheExpirationDays", settings.CacheExpirationDays);
            command.Parameters.AddWithValue("@AutoCleanCache", settings.AutoCleanCache ? 1 : 0);
            command.Parameters.AddWithValue("@DisplayMode", (int)settings.DisplayMode);
            command.Parameters.AddWithValue("@GridColumns", settings.GridColumns);
            command.Parameters.AddWithValue("@ShowWallpaperInfo", settings.ShowWallpaperInfo ? 1 : 0);
            command.Parameters.AddWithValue("@ShowWallpaperTags", settings.ShowWallpaperTags ? 1 : 0);
            command.Parameters.AddWithValue("@ThumbnailQuality", settings.ThumbnailQuality);
            command.Parameters.AddWithValue("@CreatedAt", settings.CreatedAt.ToString("o"));
            command.Parameters.AddWithValue("@UpdatedAt", settings.UpdatedAt.ToString("o"));
        }

        private async Task SaveWallpaperTagsAsync(string wallpaperId, List<Tag> tags)
        {
            if (_connection == null || tags == null || tags.Count == 0) return;

            foreach (var tag in tags)
            {
                // 保存标签
                await SaveTagAsync(tag);

                // 创建关联
                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    INSERT OR IGNORE INTO WallpaperTags (WallpaperId, TagId)
                    VALUES (@WallpaperId, @TagId)";
                command.Parameters.AddWithValue("@WallpaperId", wallpaperId);
                command.Parameters.AddWithValue("@TagId", tag.Id);
                await command.ExecuteNonQueryAsync();
            }
        }

        private Wallpaper ReadWallpaper(SqliteDataReader reader)
        {
            return new Wallpaper
            {
                Id = reader.GetString(reader.GetOrdinal("Id")),
                Url = reader.GetString(reader.GetOrdinal("Url")),
                ThumbnailUrl = reader.IsDBNull(reader.GetOrdinal("ThumbnailUrl")) ? "" : reader.GetString(reader.GetOrdinal("ThumbnailUrl")),
                SmallUrl = reader.IsDBNull(reader.GetOrdinal("SmallUrl")) ? "" : reader.GetString(reader.GetOrdinal("SmallUrl")),
                LocalPath = reader.IsDBNull(reader.GetOrdinal("LocalPath")) ? null : reader.GetString(reader.GetOrdinal("LocalPath")),
                Resolution = reader.GetString(reader.GetOrdinal("Resolution")),
                Width = reader.GetInt32(reader.GetOrdinal("Width")),
                Height = reader.GetInt32(reader.GetOrdinal("Height")),
                FileSize = reader.GetInt64(reader.GetOrdinal("FileSize")),
                FileType = reader.GetString(reader.GetOrdinal("FileType")),
                Colors = reader.IsDBNull(reader.GetOrdinal("Colors")) ? new List<string>() : reader.GetString(reader.GetOrdinal("Colors")).Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Category = (WallpaperCategory)reader.GetInt32(reader.GetOrdinal("Category")),
                Purity = (WallpaperPurity)reader.GetInt32(reader.GetOrdinal("Purity")),
                Views = reader.GetInt32(reader.GetOrdinal("Views")),
                Favorites = reader.GetInt32(reader.GetOrdinal("Favorites")),
                Downloads = reader.GetInt32(reader.GetOrdinal("Downloads")),
                UploadedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UploadedAt"))),
                DownloadedAt = reader.IsDBNull(reader.GetOrdinal("DownloadedAt")) ? null : DateTime.Parse(reader.GetString(reader.GetOrdinal("DownloadedAt"))),
                Uploader = reader.GetString(reader.GetOrdinal("Uploader")),
                UploaderAvatar = reader.IsDBNull(reader.GetOrdinal("UploaderAvatar")) ? null : reader.GetString(reader.GetOrdinal("UploaderAvatar")),
                IsFavorite = reader.GetInt32(reader.GetOrdinal("IsFavorite")) == 1,
                CollectionId = reader.IsDBNull(reader.GetOrdinal("CollectionId")) ? null : reader.GetInt32(reader.GetOrdinal("CollectionId")),
                Source = (WallpaperSource)reader.GetInt32(reader.GetOrdinal("Source")),
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")))
            };
        }

        private Collection ReadCollection(SqliteDataReader reader)
        {
            return new Collection
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Count = reader.GetInt32(reader.GetOrdinal("Count")),
                CoverUrl = reader.IsDBNull(reader.GetOrdinal("CoverUrl")) ? null : reader.GetString(reader.GetOrdinal("CoverUrl")),
                IsPublic = reader.GetInt32(reader.GetOrdinal("IsPublic")) == 1,
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt"))),
                Source = reader.IsDBNull(reader.GetOrdinal("Source")) ? null : reader.GetString(reader.GetOrdinal("Source")),
                SourceType = (CollectionSource)reader.GetInt32(reader.GetOrdinal("SourceType")),
                RemoteId = reader.IsDBNull(reader.GetOrdinal("RemoteId")) ? null : reader.GetInt32(reader.GetOrdinal("RemoteId"))
            };
        }

        private Tag ReadTag(SqliteDataReader reader)
        {
            return new Tag
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Alias = reader.IsDBNull(reader.GetOrdinal("Alias")) ? null : reader.GetString(reader.GetOrdinal("Alias")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                Category = reader.IsDBNull(reader.GetOrdinal("Category")) ? null : reader.GetString(reader.GetOrdinal("Category")),
                Url = reader.IsDBNull(reader.GetOrdinal("Url")) ? null : reader.GetString(reader.GetOrdinal("Url"))
            };
        }

        private AppSettings ReadSettings(SqliteDataReader reader)
        {
            return new AppSettings
            {
                WallhavenApiKey = reader.IsDBNull(reader.GetOrdinal("WallhavenApiKey")) ? null : reader.GetString(reader.GetOrdinal("WallhavenApiKey")),
                UnsplashApiKey = reader.IsDBNull(reader.GetOrdinal("UnsplashApiKey")) ? null : reader.GetString(reader.GetOrdinal("UnsplashApiKey")),
                PexelsApiKey = reader.IsDBNull(reader.GetOrdinal("PexelsApiKey")) ? null : reader.GetString(reader.GetOrdinal("PexelsApiKey")),
                DownloadPath = reader.GetString(reader.GetOrdinal("DownloadPath")),
                NamingRule = (FileNamingRule)reader.GetInt32(reader.GetOrdinal("NamingRule")),
                CustomNamingFormat = reader.IsDBNull(reader.GetOrdinal("CustomNamingFormat")) ? null : reader.GetString(reader.GetOrdinal("CustomNamingFormat")),
                MaxConcurrentDownloads = reader.GetInt32(reader.GetOrdinal("MaxConcurrentDownloads")),
                DownloadSpeedLimit = reader.GetInt32(reader.GetOrdinal("DownloadSpeedLimit")),
                EnableResumeDownload = reader.GetInt32(reader.GetOrdinal("EnableResumeDownload")) == 1,
                RetryCount = reader.GetInt32(reader.GetOrdinal("RetryCount")),
                AutoChangeEnabled = reader.GetInt32(reader.GetOrdinal("AutoChangeEnabled")) == 1,
                AutoChangeInterval = reader.GetInt32(reader.GetOrdinal("AutoChangeInterval")),
                ChangeSource = (WallpaperChangeSource)reader.GetInt32(reader.GetOrdinal("ChangeSource")),
                ChangeMode = (WallpaperChangeMode)reader.GetInt32(reader.GetOrdinal("ChangeMode")),
                ChangeCollectionId = reader.IsDBNull(reader.GetOrdinal("ChangeCollectionId")) ? null : reader.GetInt32(reader.GetOrdinal("ChangeCollectionId")),
                ChangeSearchId = reader.IsDBNull(reader.GetOrdinal("ChangeSearchId")) ? null : reader.GetInt32(reader.GetOrdinal("ChangeSearchId")),
                WallpaperStyle = (WallpaperStyle)reader.GetInt32(reader.GetOrdinal("WallpaperStyle")),
                SameWallpaperOnAllMonitors = reader.GetInt32(reader.GetOrdinal("SameWallpaperOnAllMonitors")) == 1,
                StartWithWindows = reader.GetInt32(reader.GetOrdinal("StartWithWindows")) == 1,
                MinimizeToTray = reader.GetInt32(reader.GetOrdinal("MinimizeToTray")) == 1,
                CloseToTray = reader.GetInt32(reader.GetOrdinal("CloseToTray")) == 1,
                Theme = (AppTheme)reader.GetInt32(reader.GetOrdinal("Theme")),
                Language = (AppLanguage)reader.GetInt32(reader.GetOrdinal("Language")),
                ShowTrayIcon = reader.GetInt32(reader.GetOrdinal("ShowTrayIcon")) == 1,
                ShowChangeNotification = reader.GetInt32(reader.GetOrdinal("ShowChangeNotification")) == 1,
                MaxSearchHistoryCount = reader.GetInt32(reader.GetOrdinal("MaxSearchHistoryCount")),
                SaveSearchHistory = reader.GetInt32(reader.GetOrdinal("SaveSearchHistory")) == 1,
                ThumbnailCachePath = reader.GetString(reader.GetOrdinal("ThumbnailCachePath")),
                MaxCacheSize = reader.GetInt32(reader.GetOrdinal("MaxCacheSize")),
                CacheExpirationDays = reader.GetInt32(reader.GetOrdinal("CacheExpirationDays")),
                AutoCleanCache = reader.GetInt32(reader.GetOrdinal("AutoCleanCache")) == 1,
                DisplayMode = (DisplayMode)reader.GetInt32(reader.GetOrdinal("DisplayMode")),
                GridColumns = reader.GetInt32(reader.GetOrdinal("GridColumns")),
                ShowWallpaperInfo = reader.GetInt32(reader.GetOrdinal("ShowWallpaperInfo")) == 1,
                ShowWallpaperTags = reader.GetInt32(reader.GetOrdinal("ShowWallpaperTags")) == 1,
                ThumbnailQuality = reader.GetInt32(reader.GetOrdinal("ThumbnailQuality")),
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")))
            };
        }

        private DownloadHistory ReadDownloadHistory(SqliteDataReader reader)
        {
            return new DownloadHistory
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                WallpaperId = reader.GetString(reader.GetOrdinal("WallpaperId")),
                WallpaperUrl = reader.GetString(reader.GetOrdinal("WallpaperUrl")),
                LocalPath = reader.GetString(reader.GetOrdinal("LocalPath")),
                FileSize = reader.GetInt64(reader.GetOrdinal("FileSize")),
                DownloadedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("DownloadedAt"))),
                Duration = reader.GetInt64(reader.GetOrdinal("Duration")),
                Success = reader.GetInt32(reader.GetOrdinal("Success")) == 1,
                ErrorMessage = reader.IsDBNull(reader.GetOrdinal("ErrorMessage")) ? null : reader.GetString(reader.GetOrdinal("ErrorMessage"))
            };
        }

        private WallpaperChangeHistory ReadChangeHistory(SqliteDataReader reader)
        {
            return new WallpaperChangeHistory
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                WallpaperId = reader.GetString(reader.GetOrdinal("WallpaperId")),
                WallpaperPath = reader.IsDBNull(reader.GetOrdinal("WallpaperPath")) ? null : reader.GetString(reader.GetOrdinal("WallpaperPath")),
                ChangedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("ChangedAt"))),
                ChangeSource = (WallpaperChangeSource)reader.GetInt32(reader.GetOrdinal("ChangeSource")),
                MonitorIndex = reader.GetInt32(reader.GetOrdinal("MonitorIndex")),
                Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? null : reader.GetString(reader.GetOrdinal("Note"))
            };
        }

        #endregion
    }
}
