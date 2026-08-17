using Process_Manager.AppLoggeres;
using Process_Manager.Display.Engine;
using Process_Manager.Enums;
using System.Diagnostics;

namespace Process_Manager.Display;

// TODO: Сделать рефактор логики.
// TODO: Сделать новые логт, точнее проверить старые, может внести конкретику.

internal class Display
{
    private bool _isDisplayList = true;

    private const int _COUNT_PROCESSES_IN_PAGE = 20;
    private int _countOfPages;
    private int _currentPage = 0;

    private readonly Lock _locker = new();
    private Process[]? _page;
    private Process[] _processes = Process.GetProcesses();
    private CancellationTokenSource _ctsDisplayList = new();
    private CancellationTokenSource _ctsUpdateList = new();
    private SortType _currentSortType = SortType.None;
    private ConsoleKeyInfo _consoleKey = default;

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
            "1. RealTime",
            "2. High",
            "3. AboveNormal",
            "4. Normal",
            "5. BelowNormal",
            "6. Idle",
       ];

    public void MainMenu()
    {
        AppLogger.Log("// - admin comments");
        AppLogger.Log("");
        while (true)
        {
            AppLogger.Log("Start method");
            AppLogger.Log("Clear console");
            Console.Clear();
            AppLogger.Log("Write logo");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(LOGO);
            Console.ResetColor();

            AppLogger.Log("Draw options");
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

            AppLogger.Log("Get user input");
            ConsoleKeyInfo consoleKey = DisplayEngine.GetHiddenUserInput();
            switch (consoleKey.Key)
            {
                case ConsoleKey.Enter:
                    {
                        AppLogger.Log("User choice: 'enter'");
                        AppLogger.Log("New ASYNC update token");
                        _ctsUpdateList = new CancellationTokenSource();
                        AppLogger.Log("New ASYNC display token");
                        _ctsDisplayList = new CancellationTokenSource();
                        AppLogger.Log("Start ASYNC method: 'UpdateProcessesAsync'");
                        _ = UpdateProcessesAsync(_ctsUpdateList);
                        AppLogger.Log("Start ASYNC method: 'ProcessesListAsync'");
                        _ = DisplayProcessesAsync(_ctsDisplayList);
                        MainDisplay();
                    }
                    break;

                case ConsoleKey.D9:
                    AppLogger.Log("User CHECK POINT");
                    break;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    AppLogger.Log("User choice: 'exit'");
                    DisplayEngine.ExitProgram();
                    break;

                default:
                    AppLogger.Log("Error: 'wrong input'");
                    DisplayError(ErrorType.Wrong_Input);
                    continue;
            }
        }
    }

    private void MainDisplay()
    {
        AppLogger.Log("Start method");

        while (true)
        {
            AppLogger.Log("Display list TRUE");
            _isDisplayList = true;
            AppLogger.Log("Get user input");


            _consoleKey = DisplayEngine.GetHiddenUserInput();
            switch (_consoleKey.Key)
            {
                case ConsoleKey.E:
                    AppLogger.Log("User choice: 'next page'");
                    if (_currentPage < _countOfPages - 1) _currentPage++;
                    break;

                case ConsoleKey.Q:
                    AppLogger.Log("User choice: 'privous page'");
                    if (_currentPage > 0) _currentPage--;
                    break;

                case ConsoleKey.Oem3:
                    {
                        AppLogger.Log("User choice: 'ProcessManage'");
                        if (!ManageProcess())
                            continue;
                    }
                    break;

                case ConsoleKey.Tab:
                    {
                        AppLogger.Log("User choice: 'ProcessFilter'");
                        if (FilterProcesses())
                            continue;
                    }
                    break;

                case ConsoleKey.D9:
                    AppLogger.Log("User CHECK POINT");
                    break;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    {
                        AppLogger.Log("User choice: 'exit'");
                        AppLogger.Log("Stop async UPDATE");
                        _ctsUpdateList.Cancel();
                        AppLogger.Log("Stop async DISPLAY");
                        _ctsDisplayList.Cancel();
                    }
                    return;

                default:
                    AppLogger.Log("Error: 'wrong input'");
                    DisplayError(ErrorType.Wrong_Input);
                    break;
            }
        }

        bool FilterProcesses()
        {
            while (true)
            {
                AppLogger.Log("FILTER: Start method: 'ProcessesFilter'");
                AppLogger.Log("FILTER: Display list FALSE");
                _isDisplayList = false;

                Console.ResetColor();
                Console.WriteLine();

                AppLogger.Log("FILTER: Draw filter options");
                for (int i = 0; i < filterOptions.Length; i++)
                    Console.WriteLine(filterOptions[i]);

                AppLogger.Log("FILTER: Get user input");
                ConsoleKeyInfo consoleKey = DisplayEngine.GetHiddenUserInput();
                switch (consoleKey.Key)
                {
                    case ConsoleKey.D1:
                        AppLogger.Log("FILTER: User choice: 'filter by name'");
                        _currentSortType = SortType.Name;
                        DisplayEngine.SortProcessesByName(_processes);
                        return true;

                    case ConsoleKey.D2:
                        AppLogger.Log("FILTER: User choice: 'filter by PID");
                        _currentSortType = SortType.PID;
                        DisplayEngine.SortProcessesByPID(_processes);
                        return true;

                    case ConsoleKey.D3:
                        AppLogger.Log("FILTER: User choice: 'filter by Memory'");
                        _currentSortType = SortType.Memory;
                        DisplayEngine.SortProcessesByMemory(_processes);
                        return true;

                    case ConsoleKey.D9:
                        AppLogger.Log("FILTER: User CHECK POINT");
                        return true;

                    case ConsoleKey.Backspace:
                    case ConsoleKey.Escape:
                        {
                            AppLogger.Log("FILTER: User choice: 'exit'");
                        }
                        return false;

                    default:
                        {
                            AppLogger.Log("FILTER: Error: 'wrong input'");
                            DisplayError(ErrorType.Wrong_Input);
                            AppLogger.Log("FILTER: Display list TRUE");
                            _isDisplayList = true;
                            Thread.Sleep(1000);
                        }
                        continue;
                }
            }
        }

        #region NotNow

        bool ManageProcess()
        {
            while (true)
            {
                AppLogger.Log("MANAGER: Start method: 'ProcessesManage'");
                Console.ResetColor();
                AppLogger.Log("MANAGER: Get user input 'CID'");
                Console.Write("Enter a CID: ");
                string? userIndexString = Console.ReadLine();

                if (!DisplayEngine.InitNumber(userIndexString ?? String.Empty, out int userIndex))
                {
                    AppLogger.Log("Error: 'wrong input'");
                    DisplayError(ErrorType.Wrong_Input);
                    continue;
                }

                AppLogger.Log("MANAGER: Draw options");
                for (int i = 0; i < processOptions.Length; i++)
                    Console.WriteLine(processOptions[i]);

                Console.Write($"\nChoose option\n");

                ConsoleKeyInfo consoleKey = DisplayEngine.GetHiddenUserInput();
                switch (consoleKey.Key)
                {
                    case ConsoleKey.D1:
                        {
                            AppLogger.Log("MANAGER: User choice: 'kill process'");
                            if (!DisplayEngine.KillProcess(_processes, userIndex))
                                DisplayError(ErrorType.Run_As_Administator);
                        }
                        return true;

                    case ConsoleKey.D2:
                        {
                            AppLogger.Log("MANAGER: User choice: 'soft kill'");
                            if (!DisplayEngine.CloseMainWindowProcess(_processes, userIndex))
                                DisplayError(ErrorType.Run_As_Administator);
                        }
                        return true;

                    case ConsoleKey.D3:
                        {
                            AppLogger.Log("MANAGER: User choice: 'Open file directory'");
                            if (!DisplayEngine.OpenFileDirectoryProcess(_processes, userIndex))
                                DisplayError(ErrorType.Run_As_Administator);
                        }
                        return true;

                    case ConsoleKey.D4:
                        {
                            AppLogger.Log("MANAGER: User choice: 'change priority'");
                            if (!ChangePriority(userIndex))
                                return false;
                        }
                        return true;

                    case ConsoleKey.D9:
                        AppLogger.Log("MANAGER: User CHECK POINT");
                        return true;

                    case ConsoleKey.Backspace:
                    case ConsoleKey.Escape:
                        {
                            AppLogger.Log("MANAGER: User choice: 'exit'"); // TODO: EMPTY LOGIC
                        }
                        return false;

                    default:
                        AppLogger.Log("Error: 'wrong input'");
                        DisplayError(ErrorType.Wrong_Input);
                        return true;

                }
            }

            bool ChangePriority(int userIndex)
            {
                while (true)
                {
                    AppLogger.Log("Start method: 'ChangePriority'");
                    AppLogger.Log("Draw options");
                    for (int i = 0; i < changePriorityOptions.Length; i++)
                        Console.WriteLine(changePriorityOptions[i]);

                    AppLogger.Log("Get user input");
                    ConsoleKeyInfo consoleKey = DisplayEngine.GetHiddenUserInput();
                    switch (consoleKey.Key)
                    {
                        case ConsoleKey.D1:
                            AppLogger.Log("User choice: 'change priority RealTime'");
                            DisplayEngine.СhangePriorityProcess(_processes, userIndex, ProcessPriorityClass.RealTime);
                            return true;

                        case ConsoleKey.D2:
                            AppLogger.Log("User choice: 'change priority RealTime'");
                            DisplayEngine.СhangePriorityProcess(_processes, userIndex, ProcessPriorityClass.High);
                            return true;

                        case ConsoleKey.D3:
                            AppLogger.Log("User choice: 'change priority RealTime'");
                            DisplayEngine.СhangePriorityProcess(_processes, userIndex, ProcessPriorityClass.AboveNormal);
                            return true;

                        case ConsoleKey.D4:
                            AppLogger.Log("User choice: 'change priority RealTime'");
                            DisplayEngine.СhangePriorityProcess(_processes, userIndex, ProcessPriorityClass.Normal);
                            return true;

                        case ConsoleKey.D5:
                            AppLogger.Log("User choice: 'change priority RealTime'");
                            DisplayEngine.СhangePriorityProcess(_processes, userIndex, ProcessPriorityClass.BelowNormal);
                            return true;

                        case ConsoleKey.D6:
                            AppLogger.Log("User choice: 'change priority RealTime'");
                            DisplayEngine.СhangePriorityProcess(_processes, userIndex, ProcessPriorityClass.Idle);
                            return true;

                        case ConsoleKey.D9:
                            AppLogger.Log("User CHECK POINT");
                            return true;

                        case ConsoleKey.Backspace:
                        case ConsoleKey.Escape:
                            AppLogger.Log("User choice: 'change priority RealTime'");
                            return false;

                        default:
                            AppLogger.Log("Error: 'wrong input'");
                            DisplayError(ErrorType.Wrong_Input);
                            return true;
                    }
                }
            }
        }
    }
        #endregion

    private async Task DisplayProcessesAsync(CancellationTokenSource tokenSource) // TODO: Переименовать
    {
        while (!tokenSource.Token.IsCancellationRequested)
        {
            if (_isDisplayList == true)
            {
                _countOfPages = _processes.Length / _COUNT_PROCESSES_IN_PAGE;

                if (_processes.Length % 10 != 0)
                    _countOfPages++;

                AppLogger.Log("ASYNC: Start method");
                double totalMemoryUsage = 0;
                var currentProcesses = _processes;

                AppLogger.Log("ASYNC: Calculate page");
                _page = [
                    ..currentProcesses
                .Skip(_COUNT_PROCESSES_IN_PAGE * _currentPage)
                .Take(_COUNT_PROCESSES_IN_PAGE)
                    ];

                if (currentProcesses == null || currentProcesses.Length == 0)
                {
                    AppLogger.Log("ASYNC: We have get null array, await 50 ms to get not null array");
                    await Task.Delay(50);
                    continue;
                }

                AppLogger.Log("ASYNC: lock display");
                lock (_locker)
                {
                    AppLogger.Log("ASYNC: Clear console");
                    Console.Clear();
                    AppLogger.Log("ASYNC: Draw header");
                    Console.WriteLine("'Q' left | 'E' right | 'TAB' filter | '`' manage | 'ESC / BACKSPACE' exit", Console.ForegroundColor = ConsoleColor.Gray);
                    Console.WriteLine($"Current page: {_currentPage + 1}|{_countOfPages}\n\n");

                    AppLogger.Log("ASYNC: Draw global stats");
                    for (int i = 0; i < currentProcesses.Length; i++)
                    {
                        totalMemoryUsage += currentProcesses[i].PrivateMemorySize64 / (1024 * 1024);

                        if (i == currentProcesses.Length - 1)
                            Console.WriteLine($"Total memory usage: {totalMemoryUsage} | Count of processes: {currentProcesses.Length}");
                    }

                    AppLogger.Log("ASYNC: Draw processes");
                    for (int i = 0; i < _page.Length; i++)
                    {
                        double memoryUsage = _page[i].PrivateMemorySize64 / (1024 * 1024); // convert byte to MB
                        string moduleFullNamePath = DisplayEngine.GetProcessModuleFullName(_page[i]);
                        string nameExtension = Path.GetExtension(moduleFullNamePath);
                        string processName = _page[i].ProcessName;
                        ConsoleColor currentColor;

                        if (i % 2 == 0) currentColor = ConsoleColor.DarkGray;
                        else currentColor = ConsoleColor.Gray;

                        if (_page[i].ProcessName.Length >= 25) processName = _page[i].ProcessName[..22] + "..." + nameExtension;
                        else processName += nameExtension;

                        Console.Write($"| CID: {currentProcesses.IndexOf(_page[i]),-2} \t", Console.ForegroundColor = currentColor);
                        Console.Write($"| Name: {processName,-25} \t", Console.ForegroundColor = ConsoleColor.Yellow);
                        Console.Write($"| PID: {_page[i].Id,-5} \t", Console.ForegroundColor = currentColor);
                        Console.Write($"| Memory: {memoryUsage} MB\n", Console.ForegroundColor = ConsoleColor.Green);
                    }
                    AppLogger.Log("ASYNC: await 950 ms");
                }
            }
            await Task.Delay(950);
        }
    }

    #region NowNow2
    private async Task UpdateProcessesAsync(CancellationTokenSource tokenSource) // TODO: перенос логики в Display Engine
    {
        while (!tokenSource.IsCancellationRequested)
        {
            AppLogger.Log("ASYNC: Get processes");
            _processes = Process.GetProcesses();

            switch (_currentSortType)
            {
                case SortType.Name:
                    AppLogger.Log("ASYNC: Sort by name");
                    DisplayEngine.SortProcessesByName(_processes); break;
                case SortType.PID:
                    AppLogger.Log("ASYNC: Sort be processor");
                    DisplayEngine.SortProcessesByPID(_processes); break;
                case SortType.Memory:
                    AppLogger.Log("ASYNC: Sort by memory");
                    DisplayEngine.SortProcessesByMemory(_processes); break;
            }

            AppLogger.Log("ASYNC: await 800 ms");
            await Task.Delay(800);
        }
    }
    #endregion

    async void DisplayError(ErrorType errorType)
    {
        AppLogger.Log("Clear console");
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;

        switch (errorType)
        {
            case ErrorType.Run_As_Administator:
                AppLogger.Log("try run as admin");
                Console.WriteLine("Error #1: Try to run the program as administrator"); break;
            case ErrorType.Wrong_Input:
                AppLogger.Log("Wrong input");
                Console.WriteLine("Error #2: Wrong input, make sure you have entered it correctly."); break;
            default:
                AppLogger.Log("Error 404");
                Console.WriteLine("Error #404: Unknown error"); break;
        }

        AppLogger.Log("Sleep thread 1500ms");
        Thread.Sleep(1500); // TODO: Как убрать проблему что пользователь нажимает H после иждет sleep и если он нажимет H то после sleep нажимется H как убрать это c#
        Console.Clear();
        Console.ResetColor();
    }
}