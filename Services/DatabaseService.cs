using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using ScanApp.Models;
using ScanApp.Services.Interfaces;

namespace ScanApp.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString = "";
        private readonly ILogger<DatabaseService> _logger;
        private readonly IPlatformService _platform;

        public DatabaseService(
            ILogger<DatabaseService> logger,
            IPlatformService platform)
        {
            _logger = logger;
            string dbPath = Path.GetFullPath(Path.Combine("Data", "scanapp.db"));
            _platform = platform;
            _connectionString = $"Data Source={dbPath};";
            EnsureDatabasePath(dbPath);
            Initialize();
        }

        public void Initialize()
        {
            using var conn = CreateConnection();
            conn.Execute(@"
                CREATE TABLE IF NOT EXISTS Files (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Path TEXT NOT NULL UNIQUE,
                    Size BIGINT NOT NULL,
                    LastModified INTEGER NOT NULL,
                    Md5Hash TEXT NOT NULL,
                    Sha1Hash TEXT NOT NULL,
                    Sha256Hash TEXT NOT NULL,
                    LastSeen INTEGER NOT NULL,
                    ScannedCount INTEGER DEFAULT 1
                );

                CREATE INDEX IF NOT EXISTS IX_Files_Hash ON Files (Sha256Hash);
            ");
        }

        public bool TryGetExistingRecord(string path, FileInfo fileInfo, out FileRecord record)
        {
            var normalizedPath = _platform.NormalizePath(path);

            using var conn = CreateConnection();
            record = conn.QueryFirstOrDefault<FileRecord>(@"
                SELECT * FROM Files 
                WHERE Path = @Path 
                AND LastModified = @LastModified
                AND Size = @Size",
                new
                {
                    Path = normalizedPath,
                    LastModified = fileInfo.LastWriteTimeUtc.ToFileTimeUtc(),
                    Size = fileInfo.Length
                });

            return record != null;
        }

        public void UpdateLastSeen(FileRecord record)
        {
            using var conn = CreateConnection();
            conn.Execute(@"
                UPDATE Files 
                SET LastSeen = @LastSeen,
                    ScannedCount = ScannedCount + 1
                WHERE Id = @Id",
                new
                {
                    Id = record.Id,
                    LastSeen = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
        }

        public void StoreFileRecord(string path, FileInfo fileInfo, FileHashes hashes)
        {
            using var conn = CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                conn.Execute(@"
                    INSERT INTO Files (
                        Path, Size, LastModified, 
                        Md5Hash, Sha1Hash, Sha256Hash, 
                        LastSeen
                    )
                    VALUES (
                        @Path, @Size, @LastModified,
                        @Md5Hash, @Sha1Hash, @Sha256Hash,
                        @LastSeen
                    )",
                    new
                    {
                        Path = _platform.NormalizePath(path),
                        Size = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTimeUtc.ToFileTimeUtc(),
                        Md5Hash = hashes.MD5,
                        Sha1Hash = hashes.SHA1,
                        Sha256Hash = hashes.SHA256,
                        LastSeen = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    }, tx);

                tx.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store file record");
                tx.Rollback();
                throw;
            }
        }

        private SqliteConnection CreateConnection() => new(_connectionString);

        private void EnsureDatabasePath(string dbPath)
        {
            var dir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}