using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ScanApp.Models;
using ScanApp.Services.Interfaces;

namespace ScanApp.Services
{
    public class FileScanner : IFileScanner
    {
        private readonly ILogger<FileScanner> _logger;
        private readonly IDatabaseService _dbService;
        private readonly IHashProvider _hashProvider;
        private readonly IFileSystemInspector _fsInspector;
        private readonly IPlatformService _platform;

        private int _totalFiles;
        private int _skippedFiles;
        private int _errorCount;
        private TimeSpan _duration;
        private double _filesPerSecond;

        public ScanResult Result => new(
            _totalFiles,
            _skippedFiles,
            _errorCount,
            _filesPerSecond,
            _duration
        );

        public FileScanner(
            IDatabaseService dbService,
            IHashProvider hashProvider,
            IFileSystemInspector fsInspector,
            IPlatformService platform,
            ILogger<FileScanner> logger)
        {
            _dbService = dbService;
            _hashProvider = hashProvider;
            _fsInspector = fsInspector;
            _platform = platform;
            _logger = logger;
        }

        public void ScanDirectory(string path)
        {
            _logger.LogInformation("Starting scan of {Directory}", path);
            var sw = Stopwatch.StartNew();

            try
            {
                var files = _fsInspector.SafeEnumerateFiles(path);
                ProcessFilesParallel(files);
            }
            catch (AggregateException ae)
            {
                HandleAggregateException(ae);
            }
            finally
            {
                FinalizeScan(sw);
            }
        }

        private void ProcessFilesParallel(IEnumerable<string> files)
        {
            Parallel.ForEach(files, new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            }, file =>
            {
                try
                {
                    ProcessSingleFile(file);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _errorCount);
                    _logger.LogError(ex, "Error processing {FilePath}", file);
                }
            });
        }

        private void ProcessSingleFile(string filePath)
        {
            if (_platform.ShouldSkipPath(filePath))
            {
                Interlocked.Increment(ref _skippedFiles);
                return;
            }

            var fileInfo = _fsInspector.GetFileInfo(filePath);

            if (_dbService.TryGetExistingRecord(filePath, fileInfo, out var existing))
            {
                _dbService.UpdateLastSeen(existing);
                Interlocked.Increment(ref _skippedFiles);
                return;
            }

            var hashes = _hashProvider.ComputeHashes(filePath);
            _dbService.StoreFileRecord(filePath, fileInfo, hashes);
            Interlocked.Increment(ref _totalFiles);
        }

        private void HandleAggregateException(AggregateException ae)
        {
            foreach (var ex in ae.Flatten().InnerExceptions)
            {
                _logger.LogError(ex, "Processing error");
                Interlocked.Increment(ref _errorCount);
            }
        }

        private void FinalizeScan(Stopwatch sw)
        {
            _duration = sw.Elapsed;
            _filesPerSecond = _totalFiles / sw.Elapsed.TotalSeconds;
            sw.Stop();
        }
    }
}