using ProcessManager.Display.Engine;
using System.Diagnostics;

namespace ProcessManager.Display;

internal class Display
{
    private event Action ListHandler;
    private event Action ExitHandler;

    private string[] options =
        {
            "1. Processes list",
            "2. Exit"
        };

    public Display()
    {
        ListHandler += ProcessesListDisplay;
        ExitHandler += DisplayEngine.Exit;
    }

    public void MainMenu()
    {
        while (true)
        {
            Console.Clear();
            for (int i = 0; i < options.Length; i++)
                Console.WriteLine(options[i]);

            ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
            switch (consoleKey.Key)
            {
                case ConsoleKey.D1: ListHandler.Invoke(); break;
                case ConsoleKey.D2: ExitHandler.Invoke(); break;
                default: WrongInputError(); continue;
            }
        }
    }

    private void ProcessesListDisplay()
    {
        var process = DisplayEngine.ProcessesListLoad();
        const int COUNT_PROCESSES_IN_PAGE = 20;
        int countOfPages = process.Length / COUNT_PROCESSES_IN_PAGE;
        int currentPage = 0;

        if (process.Length % 10 != 0)
            currentPage++;

        while (true)
        {
            Process[] page = process
            .Skip(COUNT_PROCESSES_IN_PAGE * currentPage)
            .Take(COUNT_PROCESSES_IN_PAGE)
            .ToArray();

            Console.Clear();
            Console.WriteLine("'Q' left | 'E' right | 'ESC' exit");
            Console.WriteLine($"Current page:{currentPage + 1}\n");

            for (int i = 0; i < page.Length; i++)
            {
                string processNameModifier = page[i].ProcessName;
                if (page[i].ProcessName.Length >= 25) processNameModifier = page[i].ProcessName[..25] + "...";

                Console.Write($"| Page ID: {i,-2} \t", Console.ForegroundColor = ConsoleColor.DarkGray);
                Console.Write($"| Name: {processNameModifier,-25} \t", Console.ForegroundColor = ConsoleColor.Yellow);
                Console.Write($"| Id: {page[i].Id,-5} \t", Console.ForegroundColor = ConsoleColor.DarkGray);
                Console.Write($"| VirtualMemory64: {page[i].PagedMemorySize64}\n", Console.ForegroundColor = ConsoleColor.Green);
                Console.ResetColor();
            }

            Thread.Sleep(200);
            if (!PageModifier()) return;

            bool PageModifier()
            {
                ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
                switch (consoleKey.Key)
                {
                    case ConsoleKey.E:
                        {
                            if (currentPage + 1 > countOfPages - 1) currentPage = countOfPages - 1;
                            else currentPage++;
                        }
                        ; return true;

                    case ConsoleKey.Q:
                        {
                            if (currentPage - 1 < 0) currentPage = 0;
                            else currentPage--;
                        }
                        ; return true;

                    case ConsoleKey.Backspace:
                    case ConsoleKey.Escape: return false;
                }
                return false;
            }
        }
    }

    private void WrongInputError()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error #1: Wrong user input");
        Thread.Sleep(1000);
        Console.ResetColor();
    }
}