using System.Runtime.InteropServices;

namespace ProcessManager.Models;

internal partial class NativeConsoleMethod
{
    [LibraryImport("user32.dll")]
    private static partial short GetAsyncKeyState(int vKey);

    [LibraryImport("kernel32.dll")]
    public static partial IntPtr GetStdHandle(int nStdHandle);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    public static void BlockMouseSelection()
    {
        //const int ConsoleModeAccess = 0x0040;
        const int ConsoleBlockMouseSelection = 0x0080;
        const int ConsoleAccessInputs = 0x0001 | 0x0002 | 0x0003 | 0x0004;

        const uint mode = ConsoleBlockMouseSelection | ConsoleAccessInputs;

        var consoleMode = GetStdHandle(-10);
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
            for (var i = 0; i < virtualKeyTypes.Length; i++)
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