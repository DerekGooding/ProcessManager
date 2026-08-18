using System.Runtime.CompilerServices;

namespace Process_Manager.AppLoggeres;

public static class AppLogger
{
    private static readonly Lock _locker = new();
    private const string FilePath = "processManagerLog.txt";

    public static void Log(string message, [CallerMemberName] string callerName = "")
    {
        lock (_locker)
        {
            string logLine = $"[{DateTime.Now:HH:mm:ss.fff}] | [{Environment.CurrentManagedThreadId}] | [{callerName}]: {message}\n";
            File.AppendAllText(FilePath, logLine);
        }
    }
}