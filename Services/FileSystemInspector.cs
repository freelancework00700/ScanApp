using Microsoft.Extensions.Logging;
using ScanApp.Services.Interfaces;

namespace ScanApp.Services
{
    public class FileSystemInspector : IFileSystemInspector
    {
        private readonly ILogger<FileSystemInspector> _logger;
        private readonly IPlatformService _platformService;

        public FileSystemInspector(
            ILogger<FileSystemInspector> logger,
            IPlatformService platformService)
        {
            _logger = logger;
            _platformService = platformService;
        }

        public IEnumerable<string> SafeEnumerateFiles(string path)
        {
            try
            {
                return Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(p => !_platformService.ShouldSkipPath(p));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Access denied to directory: {Path}", path);
                return Enumerable.Empty<string>();
            }
        }

        public FileInfo GetFileInfo(string path)
        {
            try
            {
                return new FileInfo(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file info for: {Path}", path);
                throw;
            }
        }

        public bool IsAccessible(string path)
        {
            try
            {
                using var fs = File.Open(path, FileMode.Open, FileAccess.Read);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}