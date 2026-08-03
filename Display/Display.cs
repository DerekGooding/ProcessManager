using ProcessManager.Display.Engine;

namespace ProcessManager.Display;

internal class Display
{
    private string[] menuOptions =
        {
            "1. Processes Menu",
            "2. Exit",
        };

    private string[] filterOptions =
       {
            "1. Filter by Name",
            "2. Filter by ID",
            "3. Filter by Memory",
       };

    private string[] processOptions =
       {
            "1. Kill Process",
            "2. Soft Kill Process",
            "3. Soft Kill Process",
       };

    public void MainMenu()
    {
        while (true)
        {
            Console.Clear();
            for (int i = 0; i < menuOptions.Length; i++)
                Console.WriteLine(menuOptions[i]);

            ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
            switch (consoleKey.Key)
            {
                case ConsoleKey.D1: ProcessesListDisplay(); break;
                case ConsoleKey.Escape:
                case ConsoleKey.D2: DisplayEngine.Exit(); break;
                default: WrongInputError(); continue;
            }
        }
    }

    private void ProcessesListDisplay() // TODO: СДЕЛАТЬ ТАК, ЧТОБЫ ОБНОВЛЯЛОСЬ В ФОНЕ, ПОКА ЖДЕТ ВВОД ПОЛЬЗОВАТЕЛЯ.
    {
        var process = DisplayEngine.ProcessesListLoad();
        const int COUNT_PROCESSES_IN_PAGE = 20;
        int countOfPages = process.Length / COUNT_PROCESSES_IN_PAGE;
        int currentPage = 0;

        if (process.Length % 10 != 0)
            countOfPages++;

        while (true)
        {
            var page = process
            .Skip(COUNT_PROCESSES_IN_PAGE * currentPage)
            .Take(COUNT_PROCESSES_IN_PAGE)
            .ToArray();

            Console.Clear();
            Console.WriteLine("'Q' left | 'E' right | 'TAB' filter | '`' manage |'ESC' exit");
            Console.WriteLine($"Current page:{currentPage + 1}\n");

            for (int i = 0; i < page.Length; i++)
            {
                double memoryUsage = page[i].WorkingSet64 / (1024 * 1024);
                string processNameModifier = page[i].ProcessName;
                if (page[i].ProcessName.Length >= 25) processNameModifier = page[i].ProcessName[..25] + "...";

                Console.Write($"| Page ID: {i,-2} \t", Console.ForegroundColor = ConsoleColor.DarkGray);
                Console.Write($"| Name: {processNameModifier,-25} \t", Console.ForegroundColor = ConsoleColor.Yellow);
                Console.Write($"| Id: {page[i].Id,-5} \t", Console.ForegroundColor = ConsoleColor.DarkGray);
                Console.Write($"| Memory: {memoryUsage} MB\n", Console.ForegroundColor = ConsoleColor.Green);
                Console.ResetColor();

                //TODO: СДЕЛАТЬ ТАК, ЧТОБЫ ПИСАЛО В ПРОЦЕССАХ не просто exitlag условно а exitlag.exe и тд тп по типу .exe .pdf и так далее. В общем обойти win exception
            }

            Thread.Sleep(200);

            ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
            switch (consoleKey.Key)
            {
                case ConsoleKey.E:
                    if (currentPage < countOfPages - 1) currentPage++;
                    break;

                case ConsoleKey.Q:
                    if (currentPage > 0) currentPage--;
                    break;

                case ConsoleKey.Tab:
                    if (!ProcessesFilter()) continue;
                    break;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape: return;
            }

            bool ProcessesFilter()
            {
                Console.Clear();

                for (int i = 0; i < filterOptions.Length; i++)
                    Console.WriteLine(filterOptions[i]);

                ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
                switch (consoleKey.Key)
                {
                    case ConsoleKey.D1:
                        DisplayEngine.SortByName(process);
                        return true;

                    case ConsoleKey.D2:
                        DisplayEngine.SortById(process);
                        return true;

                    case ConsoleKey.D3:
                        DisplayEngine.SortByMemory(process);
                        return true;

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