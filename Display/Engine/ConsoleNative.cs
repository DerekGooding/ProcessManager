using ProcessManager.Displays.Engine.NativeMethodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Process_manager.Display.Engine
{
    internal class ConsoleNative : NativeMethod
    {
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
