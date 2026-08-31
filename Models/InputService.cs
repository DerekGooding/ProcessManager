namespace ProcessManager.Models;

internal class InputService
{
    public static string GetUserMultiInput()
    {
        string userInput = Console.ReadLine() ?? String.Empty;
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