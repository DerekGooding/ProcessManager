using Process_Manager.AppLoggeres;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Process_Manager.Display.Engine;

internal class DisplayEngine
{
    // WINDOWS KERNEL32 METHODS ===============================

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAcess, bool bInheritHandle, int processId);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, ref int lpdwSize);
    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr hProcess);

    // ANDREW METHODS BELOW ===================================

    public static void Exit() =>
        Environment.Exit(0);

    public static ConsoleKeyInfo GetUserInput()
    {
        ConsoleKeyInfo userInput = Console.ReadKey(intercept: true);
        return userInput;
    }

    public static void SortByName(Process[] processes) =>
        Array.Sort(processes, (x, y) => string.Compare(x.ProcessName, y.ProcessName));

    public static void SortByPID(Process[] processes) =>
        Array.Sort(processes, (x, y) => x.Id.CompareTo(y.Id));

    public static void SortByMemory(Process[] processes) =>
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

    public static bool СhangeProcessPriority(Process[] processes, int index, ProcessPriorityClass newPriority)
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

    public static bool NumberInit(string stringEnter, out int value)
    {
        AppLogger.Log("Init user number");
        if (!int.TryParse(stringEnter, out value)) return false;
        else return true;
    }

    public static string GetModuleFullName(Process process)
    {
        int size = 512;
        var sb = new StringBuilder(size);
        IntPtr handle = OpenProcess(0x1000, false, process.Id);

        if (handle != IntPtr.Zero)
        {
            try
            {
                if (QueryFullProcessImageName(handle, 0, sb, ref size))
                {
                    string fullPath = sb.ToString();
                    return fullPath;
                }
            }

            finally
            {
                CloseHandle(handle);
            }
        }
        return string.Empty;
    }
}