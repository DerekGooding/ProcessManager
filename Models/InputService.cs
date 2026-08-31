namespace ProcessManager.Models;

internal static class InputService
{
    public static string GetUserMultiInput()
    {
        var userInput = Console.ReadLine() ?? string.Empty;
        return userInput;
    }

    public static void BlockInputInThreadSleep(int milliseconds)
    {
        Thread.Sleep(milliseconds);

        while (Console.KeyAvailable)
        {
            Console.ReadKey(true);
        }
    }
}