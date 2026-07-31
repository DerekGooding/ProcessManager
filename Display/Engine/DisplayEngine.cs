using System.Diagnostics;

namespace ProcessManager.Display.Engine;

internal class DisplayEngine
{
    public static void Exit() => Environment.Exit(0);

    public static ConsoleKeyInfo GetUserInput()
    {
        ConsoleKeyInfo userInput = Console.ReadKey(intercept: true);
        return userInput;
    }

    public static Process[] ProcessesListLoad()
    {
        Process[] process = Process.GetProcesses();
        return process;
    }
}
