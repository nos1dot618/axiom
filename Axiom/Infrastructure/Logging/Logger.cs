using System.IO;
using Axiom.Common;

namespace Axiom.Infrastructure.Logging;

public sealed class Logger(ModuleType module, LogLevel minimumLogLevel)
{
    private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.log");
    private readonly object _lock = new();

    public void Debug(string message)
    {
        Log(LogLevel.Debug, message);
    }

    public void Info(string message)
    {
        Log(LogLevel.Info, message);
    }

    public void Warning(string message)
    {
        Log(LogLevel.Warning, message);
    }

    public void Error(string message)
    {
        Log(LogLevel.Error, message);
    }

    private void Log(LogLevel level, string message)
    {
        if (level < minimumLogLevel) return;

        lock (_lock)
        {
            var logLine = $"{DateTime.Now:HH:mm:ss.fff}: [{level}] {module}: {message}\n";
            File.AppendAllText(LogFilePath, logLine);
        }
    }
}