namespace ProcessManager.Models;

internal static class InputService
{
    public static string GetUserMultiInput() => ReadLine() ?? string.Empty;

    public static void BlockInputInThreadSleep(int milliseconds)
    {
        Thread.Sleep(milliseconds);

        while (KeyAvailable)
        {
            ReadKey(true);
        }
    }
}