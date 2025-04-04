using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace ScanApp.Services.Platform
{
    public class PlatformConfigurator
    {
        private readonly ILogger<PlatformConfigurator> _logger;
        private const string AppName = "ScanApp";

        public string DatabasePath { get; }
        public string LogsDirectory { get; }
        public string AppDataDirectory { get; }

        public PlatformConfigurator(ILogger<PlatformConfigurator> logger)
        {
            _logger = logger;

            // Set platform-specific paths
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                AppDataDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    AppName);
            }
            else
            {
                AppDataDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    $".{AppName.ToLower()}");
            }

            DatabasePath = Path.Combine(AppDataDirectory, "data.db");
            LogsDirectory = Path.Combine(AppDataDirectory, "logs");

            try
            {
                EnsureDirectoriesExist();
                ValidateFilePermissions();
                _logger.LogInformation("Platform configured. Database: {DbPath}, Logs: {LogPath}",
                    DatabasePath, LogsDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Platform configuration failed");
                throw new InvalidOperationException("Platform configuration failed", ex);
            }
        }

        private void EnsureDirectoriesExist()
        {
            try
            {
                Directory.CreateDirectory(AppDataDirectory);
                Directory.CreateDirectory(LogsDirectory);

                // Ensure database directory exists
                var dbDir = Path.GetDirectoryName(DatabasePath);
                if (!string.IsNullOrEmpty(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Failed to create required directories");
                throw;
            }
        }

        private void ValidateFilePermissions()
        {
            string testFile = null!;
            try
            {
                // Test directory write access
                testFile = Path.Combine(LogsDirectory, $"access_test_{Guid.NewGuid()}.tmp");
                File.WriteAllText(testFile, "permission_test");

                // Test directory read access
                if (!File.Exists(testFile))
                {
                    throw new IOException("Failed to verify file creation");
                }
            }
            finally
            {
                try
                {
                    if (testFile != null && File.Exists(testFile))
                    {
                        File.Delete(testFile);
                    }
                }
                catch (Exception cleanEx)
                {
                    _logger.LogWarning(cleanEx, "Failed to clean up test file");
                }
            }
        }

        public bool TryGetDatabaseBackupPath(out string backupPath)
        {
            backupPath = Path.Combine(AppDataDirectory, $"backup_{DateTime.Now:yyyyMMdd}.db");
            try
            {
                if (File.Exists(DatabasePath))
                {
                    File.Copy(DatabasePath, backupPath, overwrite: true);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create database backup");
                backupPath = null!;
                return false;
            }
        }
    }
}