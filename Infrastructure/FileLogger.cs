// Infrastructure/FileLogger.cs
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace ScanApp.Infrastructure
{
    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private static readonly object _lock = new();

        public FileLogger(string filePath)
        {
            _filePath = filePath;
            EnsureDirectoryExists();
        }

        public IDisposable BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
     LogLevel logLevel,
     EventId eventId,
     TState state,
     Exception? exception,  // Add nullability marker
     Func<TState, Exception?, string> formatter)  // Update formatter signature
        {
            if (!IsEnabled(logLevel)) return;

            var message = formatter?.Invoke(state, exception) ?? string.Empty;
            var logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] {message}";

            if (exception != null)
            {
                logEntry += $"\n{exception}";
            }

            lock (_lock)
            {
                File.AppendAllText(_filePath, logEntry + Environment.NewLine);
            }
        }

        private void EnsureDirectoryExists()
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}