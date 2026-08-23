using System;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace FinTrack.Infrastructure.Logging
{
    /// <summary>
    /// Configuration for Serilog structured logging.
    /// </summary>
    public static class LoggingConfiguration
    {
        /// <summary>
        /// Configures Serilog for structured logging.
        /// </summary>
        /// <param name="logFilePath">Path to log file with date template.</param>
        public static void ConfigureSerilog(string logFilePath = "logs/fintrack-.log")
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 30)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "FinTrack.Api")
                .CreateLogger();
        }
    }
}
