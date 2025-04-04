using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScanApp.Infrastructure;
using ScanApp.Models;
using ScanApp.Services;
using ScanApp.Services.Interfaces;
using ScanApp.Services.Platform;

namespace ScanApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || !Directory.Exists(args[0]))
            {
                Console.WriteLine("Usage: ScanApp <directory-path>");
                return;
            }

            var serviceProvider = BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<Program>>();
            var scanner = serviceProvider.GetService<IFileScanner>();

            try
            {
                logger!.LogInformation("Application started");
                var sw = Stopwatch.StartNew();

                scanner!.ScanDirectory(args[0]);

                logger!.LogInformation("Scan completed in {Elapsed:ss\\.fff} seconds", sw.Elapsed);
                PrintResults(scanner.Result);
            }
            catch (Exception ex)
            {
                logger!.LogCritical(ex, "Application terminated unexpectedly");
                Environment.ExitCode = 1;
            }
            finally
            {
                (serviceProvider as IDisposable)?.Dispose();
            }
        }

        static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder => builder
                .AddConsole()
                .AddFileLogger(Path.Combine("Logs", "scanapp.log"))
                .SetMinimumLevel(LogLevel.Information));

            services.AddSingleton<IPlatformService>(provider =>
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? new WindowsPlatformService(provider.GetService<ILogger<WindowsPlatformService>>()!)
                    : new UnixPlatformService(provider.GetService<ILogger<UnixPlatformService>>()!));

            services.AddScoped<IFileScanner, FileScanner>();
            services.AddScoped<IHashProvider, HashProvider>();
            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddScoped<IFileSystemInspector, FileSystemInspector>();

            return services.BuildServiceProvider();
        }

        static void PrintResults(ScanResult result)
        {
            Console.WriteLine("\n=== Scan Results ===");
            Console.WriteLine($"Total files processed: {result.TotalFiles}");
            Console.WriteLine($"Skipped (unchanged):  {result.SkippedFiles}");
            Console.WriteLine($"Errors encountered:   {result.ErrorCount}");
            Console.WriteLine($"Processing speed:     {result.FilesPerSecond:N0} files/sec");
            Console.WriteLine($"Total duration:       {result.Duration:hh\\:mm\\:ss}");
        }
    }
}