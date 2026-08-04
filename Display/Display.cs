using ErrorTypes;
using ProcessManager.Display.Engine;
using System.Diagnostics;

namespace ProcessManager.Display;

internal class Display
{
    Process[] processes = DisplayEngine.ProcessesListLoad();

    private const string LOGO = @"
                                ..:                     
                                .-:                     
               -====             .*=         =--=       
               :--+=-            =#+        ====        
                 -:: -           :%= ++    +*+##*       
                   :+           :**-+-   ++#%=*++       
                     :#+         --=-  -*+=++           
                       #+        :--- : ===+=*          
                       - **     ==+= =  ==:=            
                 - --  :===#-+++*%#:+**#%%#::---++:- .::
             :-         -*+-*-=%%@%*%%%%+#+++ =--==:    
                            :*##%#%%%++   +=--+=*+=---=-
                               **#.-#=-==.=+*#+==       
                               =+= .:+*#%#+:.:          
                    ##=-+#*    -= . .--.#-#.: .-        
                *+#**%@#*      :=*= .***=+*#%*+ .       
              *#=*-=#++       =-=.=  +##%%++#%%%%#.++   
             #*#%%#@#*         --.   +##%%%#+%**@@#*+#+ 
            ###%%%%#           ::.   %#%%%#  ##%%%##*  
            ##%%%              :-:.  %#%%      ##      
                               =-:. #%%%              
                               =-:  *%%              
                                :
";

    private string[] menuOptions =
        {
            "Enter: Processes Menu",
            "Esc: Exit",
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
            "2. Close main process window",
            "3. Open process file directory",
       };

    public void MainMenu()
    {

        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(LOGO);
            Console.ResetColor();

            for (int i = 0; i < menuOptions.Length; i++)
            {
                int leftPartLength = 0;
                Console.SetCursorPosition(75, 12 + i);

                for (int j = 0; j < menuOptions[i].Length; j++)
                {
                    if (menuOptions[i][j] == ':' && menuOptions[i][j + 1] == ' ') leftPartLength = j;
                }

                for(int l = 0;  l < leftPartLength; l++)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(menuOptions[i][l]);
                }

                Console.ResetColor();

                for (int k = leftPartLength; k < menuOptions[i].Length; k++)
                {
                    Console.Write(menuOptions[i][k]);
                }

                Console.WriteLine();
            }

            ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
            switch (consoleKey.Key)
            {
                case ConsoleKey.Enter: ProcessesListDisplay(); break;
                case ConsoleKey.Backspace:
                case ConsoleKey.Escape: DisplayEngine.Exit(); break;
                default: DisplayError(ErrorType.Wrong_Input); continue;
            }
        }
    }

    private void ProcessesListDisplay() // TODO: СДЕЛАТЬ ТАК, ЧТОБЫ ОБНОВЛЯЛОСЬ В ФОНЕ, ПОКА ЖДЕТ ВВОД ПОЛЬЗОВАТЕЛЯ.
    {
        const int COUNT_PROCESSES_IN_PAGE = 20;
        int countOfPages = processes.Length / COUNT_PROCESSES_IN_PAGE;
        int currentPage = 0;

        if (processes.Length % 10 != 0)
            countOfPages++;

        while (true)
        {
            var page = processes
            .Skip(COUNT_PROCESSES_IN_PAGE * currentPage)
            .Take(COUNT_PROCESSES_IN_PAGE)
            .ToArray();

            Console.Clear();
            Console.WriteLine("'Q' left | 'E' right | 'TAB' filter | '`' manage |'ESC / BACKSPACE' exit", Console.ForegroundColor = ConsoleColor.Cyan);
            Console.WriteLine($"Current page:{currentPage + 1}\n");

            for (int i = 0; i < page.Length; i++)
            {
                ConsoleColor currentColor;

                if (i % 2 == 0) currentColor = ConsoleColor.DarkGray;
                else currentColor = ConsoleColor.Gray;

                double memoryUsage = page[i].WorkingSet64 / (1024 * 1024); // convert byte to MB
                string processNameModifier = page[i].ProcessName;
                if (page[i].ProcessName.Length >= 25) processNameModifier = page[i].ProcessName[..25] + "...";

                Console.Write($"| cmd ID: {processes.IndexOf(page[i]),-2} \t", Console.ForegroundColor = currentColor);
                Console.Write($"| Name: {processNameModifier,-25} \t", Console.ForegroundColor = ConsoleColor.Yellow);
                Console.Write($"| Win ID: {page[i].Id,-5} \t", Console.ForegroundColor = currentColor);
                Console.Write($"| Memory: {memoryUsage} MB\n", Console.ForegroundColor = ConsoleColor.Green);
                Console.ResetColor();
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

                case ConsoleKey.Oem3:
                    if (!ProcessesManage()) continue;
                    break;

                case ConsoleKey.Tab:
                    if (!ProcessesFilter()) continue;
                    break;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape: return;
            }

            bool ProcessesFilter()
            {
                Console.WriteLine();
                for (int i = 0; i < filterOptions.Length; i++)
                    Console.WriteLine(filterOptions[i]);

                ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
                switch (consoleKey.Key)
                {
                    case ConsoleKey.D1:
                        DisplayEngine.SortByName(processes);
                        return true;

                    case ConsoleKey.D2:
                        DisplayEngine.SortById(processes);
                        return true;

                    case ConsoleKey.D3:
                        DisplayEngine.SortByMemory(processes);
                        return true;

                    case ConsoleKey.Backspace:
                    case ConsoleKey.Escape: return false;
                }

                return false;
            }

            bool ProcessesManage()
            {
                while (true)
                {
                    Console.Write("Enter an index: ");
                    string? userIndexString = Console.ReadLine();

                    if (!DisplayEngine.NumberCheck(userIndexString ?? String.Empty, out int userIndex))
                    {
                        DisplayError(ErrorType.Wrong_Input);
                        continue;
                    }

                    Console.WriteLine();

                    for (int i = 0; i < processOptions.Length; i++)
                        Console.WriteLine(processOptions[i]);

                    Console.WriteLine("Choose option");

                    ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
                    switch (consoleKey.Key)
                    {
                        case ConsoleKey.D1:
                            {
                                if (!DisplayEngine.KillProcess(processes, userIndex))
                                    DisplayError(ErrorType.Run_As_Administator);
                            }
                            return true;

                        case ConsoleKey.D2:
                            {
                                if (!DisplayEngine.CloseMainWindowProcess(processes, userIndex))
                                    DisplayError(ErrorType.Run_As_Administator);
                            }
                            return true;

                        case ConsoleKey.D3:
                            {
                                if (!DisplayEngine.OpenFileDirectoryProcess(processes, userIndex))
                                    DisplayError(ErrorType.Run_As_Administator);
                            }
                            return true;

                        case ConsoleKey.Backspace:
                        case ConsoleKey.Escape: return false;
                    }

                    return false;
                }
            }
        }
    }

    private void DisplayError(ErrorType errorType)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(Enum.GetName(errorType));
        Thread.Sleep(1000);
        Console.ResetColor();
    }
}