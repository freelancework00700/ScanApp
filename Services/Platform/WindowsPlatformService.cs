using System;
using System.IO;
using Microsoft.Extensions.Logging;
using ScanApp.Services.Interfaces;

namespace ScanApp.Services.Platform
{
    public class WindowsPlatformService : IPlatformService
    {
        private readonly ILogger<WindowsPlatformService> _logger;

        public WindowsPlatformService(ILogger<WindowsPlatformService> logger)
        {
            _logger = logger;
        }

        public string NormalizePath(string path) => Path.GetFullPath(path).ToUpperInvariant();

        public bool ShouldSkipPath(string path)
        {
            try
            {
                var attr = File.GetAttributes(path);
                if ((attr & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    _logger.LogDebug("Skipping hidden file: {File}", path);
                    return true;
                }

                if (path.Contains("\\Windows\\", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Skipping system directory: {Directory}", path);
                    return true;
                }

                return false;
            }
            catch
            {
                return true;
            }
        }

        public FileAttributes GetExtendedAttributes(string path)
        {
            try
            {
                return File.GetAttributes(path);
            }
            catch
            {
                return FileAttributes.Normal;
            }
        }
    }
}