using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Process_manager.Engine;

internal class NativeProcessService
{
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAcess, bool bInheritHandle, int processId);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, ref int lpdwSize);
    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr hProcess);

    public static string GetProcessModuleFullName(Process process)
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

    public static bool CheckProcessPointer(Process process)
    {
        nint intPtr = OpenProcess(0x1000, false, process.Id);
        if (intPtr == null)
        {
            return true;
        }

        return false;
    }
}
