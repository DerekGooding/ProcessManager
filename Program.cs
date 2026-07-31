using System.Diagnostics;
using ProcessManager.Display;

class Program
{
    static void Main()
    {
        Console.Title = "Process manager";

        var display = new Display();
        display.MainMenu();
    }
}