namespace ProcessManager.Models.InputServices;

internal class InputService
{
    public static ConsoleKeyInfo GetHiddenUserInput()
    {
        ConsoleKeyInfo userInput = Console.ReadKey(intercept: true);
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