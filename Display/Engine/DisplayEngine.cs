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

    public static void SortByName(Process[] processes)
    {
        Array.Sort(processes, (x, y) => string.Compare(x.ProcessName, y.ProcessName));
    }

    public static void SortById(Process[] processes)
    {
        Array.Sort(processes, (x, y) => x.Id.CompareTo(y.Id));
    }

    public static void SortByMemory(Process[] processes)
    {
        Array.Sort(processes, (x, y) => y.WorkingSet64.CompareTo(x.WorkingSet64));
    }
}
