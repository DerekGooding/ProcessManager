using System.Runtime.InteropServices;

namespace Process_manager.Module
{
    internal class NativeConsoleMethod
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public static void BlockMouseSelection()
        {
            const int ConsoleModeAccess = 0x0040;
            const int ConsoleBlockMouseSelection = 0x0080;
            const int ConsoleAccessInputs = 0x0001 | 0x0002 | 0x0003 | 0x0004;

            uint mode = ConsoleBlockMouseSelection | ConsoleAccessInputs;

            IntPtr consoleMode = GetStdHandle(-10);
            SetConsoleMode(consoleMode, mode);
        }
    }
}
