// Infrastructure/FileLoggerProvider.cs
using Microsoft.Extensions.Logging;
using System;

namespace ScanApp.Infrastructure
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;

        public FileLoggerProvider(string filePath)
        {
            _filePath = filePath;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_filePath);
        }

        public void Dispose()
        {
            // No cleanup needed
        }
    }
}