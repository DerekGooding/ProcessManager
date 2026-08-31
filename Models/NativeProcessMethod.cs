using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ProcessManager.Models;

internal static partial class NativeProcessService
{
    [LibraryImport("kernel32.dll")]
    public static partial IntPtr OpenProcess(int dwDesiredAcess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int processId);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, ref int lpdwSize);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool CloseHandle(IntPtr hProcess);

    public static string GetProcessModuleFullName(Process process)
    {
        var size = 512;
        var sb = new StringBuilder(size);
        var handle = OpenProcess(0x1000, false, process.Id);

        if (handle != IntPtr.Zero)
        {
            try
            {
                if (QueryFullProcessImageName(handle, 0, sb, ref size))
                {
                    return sb.ToString();
                }
            }
            finally
            {
                CloseHandle(handle);
            }
        }
        return string.Empty;
    }

    public static bool CheckProcessName(Process process) => process == null;
}