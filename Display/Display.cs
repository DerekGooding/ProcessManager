using System.Diagnostics;
using ProcessManager.Display.Engine;

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
                default: Console.WriteLine("Menu wrong input"); break;
            }
        }
    }

    private void ProcessesListDisplay()
    {
        var process = DisplayEngine.ProcessesListLoad();
        const int COUNT_PROCESSES_IN_PAGE = 20;
        int countOfPages = process.Length / COUNT_PROCESSES_IN_PAGE + 1;
        int currentPage = 0;

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
                Console.WriteLine($"№: {i} \t {page[i].Id}\t| {page[i].ProcessName}");
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
                        ;return true;

                    case ConsoleKey.Q:
                        {
                            if (currentPage - 1 < 0) currentPage = 0;
                            else currentPage--;
                        }
                        ;return true;

                    case ConsoleKey.Escape: return false;
                }
                return false;
            }
        }
    }
}