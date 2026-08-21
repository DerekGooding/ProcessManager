using ProcessManager.Displays.Engine.NativeMethodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Process_manager.Display.Engine
{
    internal class ProcessNative : NativeMethod
    {
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

    }
}
