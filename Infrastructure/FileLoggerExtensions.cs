// Infrastructure/FileLoggerExtensions.cs
using Microsoft.Extensions.Logging;

namespace ScanApp.Infrastructure
{
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFileLogger(
            this ILoggingBuilder builder,
            string filePath)
        {
            builder.AddProvider(new FileLoggerProvider(filePath));
            return builder;
        }
    }
}