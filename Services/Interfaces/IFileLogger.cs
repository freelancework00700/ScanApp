using Microsoft.Extensions.Logging;

namespace ScanApp.Services.Interfaces
{
    public interface IFileLogger : ILogger
    {
        void ArchiveLogs();
        string CurrentLogPath { get; }
    }
}