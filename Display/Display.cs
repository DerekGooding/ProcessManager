using Process_Manager.Display.Engine;
using ProcessManger.AppLogger;
using ProcessManger.Enums;
using System.Diagnostics;

namespace Process_Manager.Display;

internal class Display
{
    const int COUNT_PROCESSES_IN_PAGE = 20;
    int countOfPages;
    int currentPage = 0;

    readonly Lock locker = new();
    Process[]? page;
    Process[] processes = Process.GetProcesses();
    CancellationTokenSource ctsDisplayList = new();
    CancellationTokenSource ctsUpdateList = new();
    SortType currentSortType = SortType.None;
    ConsoleKeyInfo consoleKey = default;

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

    private readonly string[] menuOptions =
        [
            "Enter: Processes Menu",
            "Esc: Exit",
        ];

    private readonly string[] filterOptions =
       [
            "1. Filter by Name",
            "2. Filter by PID",
            "3. Filter by Memory",
       ];

    private readonly string[] processOptions =
       [
            "1. Kill Process",
            "2. Close main process window",
            "3. Open process file directory",
            "4. Change priority of process"
       ];

    private readonly string[] changePriorityOptions =
       [
            "1. RealTime ( MAX )",
            "2. High",
            "3. AboveNormal",
            "4. Normal",
            "5. BelowNormal",
            "6. Idle ( AFK )",
       ];

    public void MainMenu()
    {
        AppLogger.LogDebug("// - admin comments");
        AppLogger.LogDebug("");
        while (true)
        {
            AppLogger.LogDebug("Start method");
            AppLogger.LogDebug("Clear console");
            Console.Clear();
            AppLogger.LogDebug("Write logo");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(LOGO);
            Console.ResetColor();

            AppLogger.LogDebug("Draw options");
            for (int i = 0; i < menuOptions.Length; i++)
            {
                int leftPartLength = 0;
                int xPositionCursor = 75;
                int yPositionCursor = 12;

                Console.SetCursorPosition(xPositionCursor, yPositionCursor + i);

                for (int j = 0; j < menuOptions[i].Length; j++)
                    if (menuOptions[i][j] == ':') leftPartLength = j;

                for (int l = 0; l < leftPartLength; l++)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(menuOptions[i][l]);
                }

                Console.ResetColor();

                for (int k = leftPartLength; k < menuOptions[i].Length; k++)
                    Console.Write(menuOptions[i][k]);
            }

            AppLogger.LogDebug("Get user input");
            ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
            switch (consoleKey.Key)
            {
                case ConsoleKey.Enter:
                    {
                        AppLogger.LogDebug("User choice: 'enter'");
                        AppLogger.LogDebug("New ASYNC update token");
                        ctsUpdateList = new CancellationTokenSource();
                        AppLogger.LogDebug("New ASYNC display token");
                        ctsDisplayList = new CancellationTokenSource();
                        AppLogger.LogDebug("Start ASYNC method: 'UpdateProcessesAsync'");
                        _ = UpdateProcessesAsync(ctsUpdateList);
                        AppLogger.LogDebug("Start ASYNC method: 'ProcessesListAsync'");
                        _ = ProcessesListAsync(ctsDisplayList);
                        ProcessesListDisplay();
                    }
                    break; // NOTE: Here was an await warning!

                case ConsoleKey.D9:
                    AppLogger.LogDebug("User CHECK POINT");
                    break;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    AppLogger.LogDebug("User choice: 'exit'");
                    DisplayEngine.Exit();
                    break;

                default:
                    AppLogger.LogDebug("Error: 'wrong input'");
                    DisplayError(ErrorType.Wrong_Input);
                    continue;
            }
        }
    }

    private void ProcessesListDisplay()
    {
        AppLogger.LogDebug("Start method");

        while (true)
        {
            AppLogger.LogDebug("Get user input");
            consoleKey = DisplayEngine.GetUserInput();

            switch (consoleKey.Key)
            {
                case ConsoleKey.E:
                    AppLogger.LogDebug("User choice: 'next page'");
                    if (currentPage < countOfPages - 1) currentPage++;
                    break;

                case ConsoleKey.Q:
                    AppLogger.LogDebug("User choice: 'privous page'");
                    if (currentPage > 0) currentPage--;
                    break;

                case ConsoleKey.Oem3:
                    {
                        AppLogger.LogDebug("User choice: 'ProcessManage'");
                        if (!DisplayEngine.UserMenu(ref ctsUpdateList, ref ctsDisplayList, ProcessesManage, ProcessesListAsync, UpdateProcessesAsync))
                            continue;
                    }
                    break;

                case ConsoleKey.Tab:
                    {
                        AppLogger.LogDebug("User choice: 'ProcessFilter'");
                        if (!DisplayEngine.UserMenu(ref ctsUpdateList, ref ctsDisplayList, ProcessesFilter, ProcessesListAsync, UpdateProcessesAsync))
                            continue;
                    }
                    break;

                case ConsoleKey.D9:
                    AppLogger.LogDebug("User CHECK POINT");
                    break;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    {
                        AppLogger.LogDebug("User choice: 'exit'");
                        AppLogger.LogDebug("Stop async UPDATE");
                        ctsUpdateList.Cancel();
                        AppLogger.LogDebug("Stop async DISPLAY");
                        ctsDisplayList.Cancel();
                    }
                    return;

                default:
                    AppLogger.LogDebug("Error: 'wrong input'");
                    DisplayError(ErrorType.Wrong_Input);
                    break;
            }
        }

        bool ProcessesFilter()
        {
            AppLogger.LogDebug("FILTER: Start method: 'ProcessesFilter'");
            Console.ResetColor();
            Console.WriteLine();

            AppLogger.LogDebug("FILTER: Draw filter options");
            for (int i = 0; i < filterOptions.Length; i++)
                Console.WriteLine(filterOptions[i]);

            AppLogger.LogDebug("FILTER: Get user input");
            ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
            switch (consoleKey.Key)
            {
                case ConsoleKey.D1:
                    AppLogger.LogDebug("FILTER: User choice: 'filter by name'");
                    currentSortType = SortType.Name;
                    DisplayEngine.SortByName(processes);
                    return true;

                case ConsoleKey.D2:
                    AppLogger.LogDebug("FILTER: User choice: 'filter by PID");
                    currentSortType = SortType.PID;
                    DisplayEngine.SortByPID(processes);
                    return true;

                case ConsoleKey.D3:
                    AppLogger.LogDebug("FILTER: User choice: 'filter by Memory'");
                    currentSortType = SortType.Memory;
                    DisplayEngine.SortByMemory(processes);
                    return true;

                case ConsoleKey.D9:
                    AppLogger.LogDebug("FILTER: User CHECK POINT");
                    return true;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    {
                        AppLogger.LogDebug("FILTER: User choice: 'exit'");
                        ctsDisplayList = new CancellationTokenSource();
                        taskDisplayList = ProcessesListAsync(ctsDisplayList);
                    }
                    return false;

                default:
                    AppLogger.LogDebug("Error: 'wrong input'");
                    DisplayError(ErrorType.Wrong_Input);
                    return true;

            }
        }

        bool ProcessesManage()
        {
            while (true)
            {
                AppLogger.LogDebug("MANAGER: Start method: 'ProcessesManage'");
                Console.ResetColor();
                AppLogger.LogDebug("MANAGER: Get user input 'CID'");
                Console.Write("Enter a CID: ");
                string? userIndexString = Console.ReadLine();

                if (!DisplayEngine.NumberInit(userIndexString ?? String.Empty, out int userIndex))
                {
                    AppLogger.LogDebug("Error: 'wrong input'");
                    DisplayError(ErrorType.Wrong_Input);
                    continue;
                }

                AppLogger.LogDebug("MANAGER: Draw options");
                for (int i = 0; i < processOptions.Length; i++)
                    Console.WriteLine(processOptions[i]);

                Console.Write($"\nChoose option\n");

                ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
                switch (consoleKey.Key)
                {
                    case ConsoleKey.D1:
                        {
                            AppLogger.LogDebug("MANAGER: User choice: 'kill process'");
                            if (!DisplayEngine.KillProcess(processes, userIndex))
                                DisplayError(ErrorType.Run_As_Administator);
                        }
                        return true;

                    case ConsoleKey.D2:
                        {
                            AppLogger.LogDebug("MANAGER: User choice: 'soft kill'");
                            if (!DisplayEngine.CloseMainWindowProcess(processes, userIndex))
                                DisplayError(ErrorType.Run_As_Administator);
                        }
                        return true;

                    case ConsoleKey.D3:
                        {
                            AppLogger.LogDebug("MANAGER: User choice: 'Open file directory'");
                            if (!DisplayEngine.OpenFileDirectoryProcess(processes, userIndex))
                                DisplayError(ErrorType.Run_As_Administator);
                        }
                        return true;

                    case ConsoleKey.D4:
                        {
                            AppLogger.LogDebug("MANAGER: User choice: 'change priority'");
                            if (!ChangePriority(userIndex))
                                return false;
                        }
                        return true;

                    case ConsoleKey.D9:
                        AppLogger.LogDebug("MANAGER: User CHECK POINT");
                        return true;

                    case ConsoleKey.Backspace:
                    case ConsoleKey.Escape:
                        {
                            AppLogger.LogDebug("MANAGER: User choice: 'exit'");
                            ctsDisplayList = new CancellationTokenSource();
                            taskDisplayList = ProcessesListAsync(ctsDisplayList);
                        }
                        return false;

                    default:
                        AppLogger.LogDebug("Error: 'wrong input'");
                        DisplayError(ErrorType.Wrong_Input);
                        return true;
                }
            }

            bool ChangePriority(int userIndex)
            {
                while (true)
                {
                    AppLogger.LogDebug("Start method: 'ChangePriority'");
                    AppLogger.LogDebug("Draw options");
                    for (int i = 0; i < changePriorityOptions.Length; i++)
                        Console.WriteLine(changePriorityOptions[i]);

                    AppLogger.LogDebug("Get user input");
                    ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
                    switch (consoleKey.Key)
                    {
                        case ConsoleKey.D1:
                            AppLogger.LogDebug("User choice: 'change priority RealTime'");
                            DisplayEngine.СhangeProcessPriority(processes, userIndex, ProcessPriorityClass.RealTime);
                            return true;

                        case ConsoleKey.D2:
                            AppLogger.LogDebug("User choice: 'change priority RealTime'");
                            DisplayEngine.СhangeProcessPriority(processes, userIndex, ProcessPriorityClass.High);
                            return true;

                        case ConsoleKey.D3:
                            AppLogger.LogDebug("User choice: 'change priority RealTime'");
                            DisplayEngine.СhangeProcessPriority(processes, userIndex, ProcessPriorityClass.AboveNormal);
                            return true;

                        case ConsoleKey.D4:
                            AppLogger.LogDebug("User choice: 'change priority RealTime'");
                            DisplayEngine.СhangeProcessPriority(processes, userIndex, ProcessPriorityClass.Normal);
                            return true;

                        case ConsoleKey.D5:
                            AppLogger.LogDebug("User choice: 'change priority RealTime'");
                            DisplayEngine.СhangeProcessPriority(processes, userIndex, ProcessPriorityClass.BelowNormal);
                            return true;

                        case ConsoleKey.D6:
                            AppLogger.LogDebug("User choice: 'change priority RealTime'");
                            DisplayEngine.СhangeProcessPriority(processes, userIndex, ProcessPriorityClass.Idle);
                            return true;

                        case ConsoleKey.D9:
                            AppLogger.LogDebug("User CHECK POINT");
                            return true;

                        case ConsoleKey.Backspace:
                        case ConsoleKey.Escape:
                            AppLogger.LogDebug("User choice: 'change priority RealTime'");
                            return false;

                        default:
                            AppLogger.LogDebug("Error: 'wrong input'");
                            DisplayError(ErrorType.Wrong_Input);
                            return true;
                    }
                }
            }
        }
    }

    private async Task ProcessesListAsync(CancellationTokenSource tokenSource)
    {
        while (!tokenSource.Token.IsCancellationRequested)
        {
            countOfPages = processes.Length / COUNT_PROCESSES_IN_PAGE;

            if (processes.Length % 10 != 0)
                countOfPages++;

            AppLogger.LogDebug("ASYNC: Start method");
            double totalMemoryUsage = 0;
            var currentProcesses = processes;

            AppLogger.LogDebug("ASYNC: Calculate page");
            page = [..currentProcesses
                .Skip(COUNT_PROCESSES_IN_PAGE * currentPage)
                .Take(COUNT_PROCESSES_IN_PAGE)];

            if (currentProcesses == null || currentProcesses.Length == 0)
            {
                AppLogger.LogDebug("ASYNC: We have get null array, await 50 ms to get not null array");
                await Task.Delay(50);
                continue;
            }

            AppLogger.LogDebug("ASYNC: lock display");
            lock (locker)
            {
                AppLogger.LogDebug("ASYNC: Clear console");
                Console.Clear();
                AppLogger.LogDebug("ASYNC: Draw header");
                Console.WriteLine("'Q' left | 'E' right | 'TAB' filter | '`' manage |'ESC / BACKSPACE' exit", Console.ForegroundColor = ConsoleColor.Gray);
                Console.WriteLine($"Current page: {currentPage + 1}|{countOfPages}\n\n");

                AppLogger.LogDebug("ASYNC: Draw global stats");
                for (int i = 0; i < currentProcesses.Length; i++)
                {
                    totalMemoryUsage += currentProcesses[i].PrivateMemorySize64 / (1024 * 1024);

                    if (i == currentProcesses.Length - 1)
                        Console.WriteLine($"Total memory usage: {totalMemoryUsage} | Count of processes: {currentProcesses.Length}");
                }

                AppLogger.LogDebug("ASYNC: Draw processes");
                for (int i = 0; i < page.Length; i++)
                {
                    double memoryUsage = page[i].PrivateMemorySize64 / (1024 * 1024); // convert byte to MB
                    string moduleFullNamePath = DisplayEngine.GetModuleFullName(page[i]);
                    string nameExtension = Path.GetExtension(moduleFullNamePath);
                    string processName = page[i].ProcessName;
                    ConsoleColor currentColor;

                    if (i % 2 == 0) currentColor = ConsoleColor.DarkGray;
                    else currentColor = ConsoleColor.Gray;

                    if (page[i].ProcessName.Length >= 25) processName = page[i].ProcessName[..22] + "..." + nameExtension;
                    else processName += nameExtension;

                    Console.Write($"| CID: {currentProcesses.IndexOf(page[i]),-2} \t", Console.ForegroundColor = currentColor);
                    Console.Write($"| Name: {processName,-25} \t", Console.ForegroundColor = ConsoleColor.Yellow);
                    Console.Write($"| PID: {page[i].Id,-5} \t", Console.ForegroundColor = currentColor);
                    Console.Write($"| Memory: {memoryUsage} MB\n", Console.ForegroundColor = ConsoleColor.Green);
                }
                AppLogger.LogDebug("ASYNC: await 950 ms");
            }
            await Task.Delay(950);
        }
    }

    private async Task UpdateProcessesAsync(CancellationTokenSource tokenSource)
    {
        while (!tokenSource.IsCancellationRequested)
        {
            AppLogger.LogDebug("ASYNC: Get processes");
            processes = Process.GetProcesses();

            switch (currentSortType)
            {
                case SortType.Name:
                    AppLogger.LogDebug("ASYNC: Sort by name");
                    DisplayEngine.SortByName(processes); break;
                case SortType.PID:
                    AppLogger.LogDebug("ASYNC: Sort be processor");
                    DisplayEngine.SortByPID(processes); break;
                case SortType.Memory:
                    AppLogger.LogDebug("ASYNC: Sort by memory");
                    DisplayEngine.SortByMemory(processes); break;
            }
            AppLogger.LogDebug("ASYNC: await 900 ms");
            await Task.Delay(900);
        }
    }

    private static void DisplayError(ErrorType errorType)
    {
        AppLogger.LogDebug("Clear console");
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;

        switch (errorType)
        {
            case ErrorType.Run_As_Administator:
                AppLogger.LogDebug("try run as admin");
                Console.WriteLine("Error #1: Try to run the program as administrator"); break;
            case ErrorType.Wrong_Input:
                AppLogger.LogDebug("Wrong input");
                Console.WriteLine("Error #2: Wrong input, make sure you have entered it correctly."); break;
            default:
                AppLogger.LogDebug("Error 404");
                Console.WriteLine("Error #404: Unknown error"); break;
        }

        AppLogger.LogDebug("Sleep thread 1500ms");
        Thread.Sleep(1500);
        Console.ResetColor();
    }
}