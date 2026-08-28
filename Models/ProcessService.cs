using ProcessManager.Loggers.AppLoggeres;
using System.ComponentModel;
using System.Diagnostics;

namespace ProcessManager.Models.ProcessServices;

internal class ProcessService
{
    public static void SortProcessesByName(ref Process[] processes) =>
        Array.Sort(processes, (x, y) => string.Compare(x.ProcessName, y.ProcessName));

    public static void SortProcessesByPid(ref Process[] processes) =>
        Array.Sort(processes, (x, y) => x.Id.CompareTo(y.Id));

    public static void SortProcessesByMemory(ref Process[] processes) =>
        Array.Sort(processes, (x, y) => y.PrivateMemorySize64.CompareTo(x.PrivateMemorySize64));


    public static bool KillProcess(Process[] processes, int index)
    {
        try
        {
            AppLogger.Log("kill process engine");
            processes[index].Kill();
            return true;
        }

        catch (Win32Exception)
        {
            AppLogger.Log("Win 32 exception");
            return false;
        }
    }

    public static bool CloseMainWindowProcess(Process[] processes, int index)
    {
        try
        {
            AppLogger.Log("soft close engine");
            processes[index].CloseMainWindow();
            return true;
        }

        catch (Win32Exception)
        {
            AppLogger.Log("Win 32 exception");
            return false;
        }
    }

    public static bool OpenFileDirectoryProcess(Process[] processes, int index)
    {
        try
        {
            AppLogger.Log("get process full path engine");
            string processFilePath = processes[index]?.MainModule?.FileName ?? String.Empty;

            if (processFilePath == String.Empty)
                return false;

            Process.Start("explorer.exe", $"/select,\"{processFilePath}\"");
            return true;
        }

        catch (Win32Exception)
        {
            AppLogger.Log("Win 32 exception");
            return false;
        }
    }

    public static bool ChangePriorityProcess(Process[] processes, int index, ProcessPriorityClass newPriority)
    {
        try
        {
            AppLogger.Log("Change priority engine");
            processes[index].PriorityClass = newPriority;
            return true;
        }
        catch (Win32Exception)
        {
            AppLogger.Log("Win 32 exception");
            return false;
        }
    }
}
