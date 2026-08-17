using System.Runtime.CompilerServices;

namespace Process_Manager.AppLoggeres;

public static class AppLogger
{
    private static readonly object _logLock = new object();
    private const string FilePath = "processManagerLog.txt";

    public static void LogDebug(string message, [CallerMemberName] string callerName = "")
    {
        lock (_logLock)
        {
            string logLine = $"[{DateTime.Now:HH:mm:ss.fff}] | [{Thread.CurrentThread.ManagedThreadId}] | [{callerName}]: {message}\n";
            File.AppendAllText(FilePath, logLine);
        }
    }
}