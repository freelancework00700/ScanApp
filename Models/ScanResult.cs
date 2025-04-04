namespace ScanApp.Models
{
    public sealed class ScanResult
    {
        public int TotalFiles { get; }
        public int SkippedFiles { get; }
        public int ErrorCount { get; }
        public double FilesPerSecond { get; }
        public TimeSpan Duration { get; }

        public ScanResult(
            int totalFiles,
            int skippedFiles,
            int errorCount,
            double filesPerSecond,
            TimeSpan duration)
        {
            TotalFiles = totalFiles;
            SkippedFiles = skippedFiles;
            ErrorCount = errorCount;
            FilesPerSecond = filesPerSecond;
            Duration = duration;
        }
    }
}