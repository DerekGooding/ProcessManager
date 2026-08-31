using System.ComponentModel;
using System.Diagnostics;

namespace ProcessManager.Models;

internal static class ProcessService
{
    public static Process[] GetAllProcesses() => Process.GetProcesses();

    public static void SortProcessesByName(Process[] processes) =>
        Array.Sort(processes, (x, y) => string.Compare(x.ProcessName, y.ProcessName));

    public static void SortProcessesByPid(Process[] processes) =>
        Array.Sort(processes, (x, y) => x.Id.CompareTo(y.Id));

    public static void SortProcessesByMemory(Process[] processes) =>
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
            var processFilePath = processes[index]?.MainModule?.FileName ?? string.Empty;

            if (processFilePath.Length == 0)
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

    public static float CalculateTotalMemoryUsage(Process[] processes)
    {
        float totalMemoryUsage = 0;

        for (var i = 0; i < processes.Length; i++)
            totalMemoryUsage += (float)processes[i].PrivateMemorySize64 / (1024 * 1024);

        return totalMemoryUsage;
    }

    public static string BuildProcessName(Process process)
    {
        if (process == null)
        {
            return string.Empty;
        }

        var moduleFullNamePath = NativeProcessService.GetProcessModuleFullName(process);
        var nameExtension = Path.GetExtension(moduleFullNamePath);
        return process.ProcessName.Length >= 25 ? process.ProcessName[..22] + "..." + nameExtension : process.ProcessName + nameExtension;
    }

    public static int CalculateProcessMemoryUsage(Process process)
        => process == null ? -1 : (int)(process.PrivateMemorySize64 / (1024 * 1024));

    public static bool CheckProcessNamePointer(Process process, int index) => NativeProcessService.CheckProcessName(process);
}