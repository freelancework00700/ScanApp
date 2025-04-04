using System;
using System.IO;
using Microsoft.Extensions.Logging;
using ScanApp.Services.Interfaces;

namespace ScanApp.Services.Platform
{
    public class UnixPlatformService : IPlatformService
    {
        private readonly ILogger<UnixPlatformService> _logger;

        public UnixPlatformService(ILogger<UnixPlatformService> logger)
        {
            _logger = logger;
        }

        public string NormalizePath(string path) => Path.GetFullPath(path);

        public bool ShouldSkipPath(string path)
        {
            var fileName = Path.GetFileName(path);
            if (fileName.StartsWith(".", StringComparison.Ordinal))
            {
                _logger.LogDebug("Skipping hidden file: {File}", path);
                return true;
            }

            var directory = Path.GetDirectoryName(path);
            if (directory?.Contains("/proc/", StringComparison.Ordinal) == true)
            {
                _logger.LogWarning("Skipping system directory: {Directory}", directory);
                return true;
            }

            return false;
        }

        public FileAttributes GetExtendedAttributes(string path) => FileAttributes.Normal;
    }
}