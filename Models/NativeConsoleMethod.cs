using ProcessManager.Enums;
using System.Runtime.InteropServices;

namespace ProcessManager.Models;

internal class NativeConsoleMethod
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetStdHandle(int nStdHandle);
    [DllImport("kernel32.dll")]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    public static void BlockMouseSelection()
    {
        //const int ConsoleModeAccess = 0x0040;
        const int ConsoleBlockMouseSelection = 0x0080;
        const int ConsoleAccessInputs = 0x0001 | 0x0002 | 0x0003 | 0x0004;

        uint mode = ConsoleBlockMouseSelection | ConsoleAccessInputs;

        IntPtr consoleMode = GetStdHandle(-10);
        SetConsoleMode(consoleMode, mode);
    }

    public static VirtualKeyType GetHiddenUserInput()
    {
        //VirtualKeyType virtualKeyType = VirtualKeyType.VK_NONE;

        VirtualKeyType[] virtualKeyTypes = [VirtualKeyType.VK_1, VirtualKeyType.VK_2, VirtualKeyType.VK_3, VirtualKeyType.VK_4,
            VirtualKeyType.VK_5, VirtualKeyType.VK_6, VirtualKeyType.VK_ESCAPE, VirtualKeyType.VK_BACK, VirtualKeyType.VK_Q, VirtualKeyType.VK_E,
            VirtualKeyType.VK_F1, VirtualKeyType.VK_OEM_3, VirtualKeyType.VK_RETURN, VirtualKeyType.VK_TAB,
            ];

        while (true)
        {
            for (int i = 0; i < virtualKeyTypes.Length; i++)
            {
                if ((GetAsyncKeyState((short)virtualKeyTypes[i]) & 0x8000) != 0)
                {
                    while ((GetAsyncKeyState((short)virtualKeyTypes[i]) & 0x0001) != 0)
                    {
                        Thread.Sleep(2);
                    }

                    return virtualKeyTypes[i];
                }

                Thread.Sleep(2);
            }
        }
    }
}