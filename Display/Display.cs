// TODO: Сделать рефактор логики.
// TODO: Сделать новые логи, точнее проверить старые, может внести конкретику и где то что то добавить.
// TODO: Выпустить релиз приложения с log vers и без log просто vers
// TODO: Написать README для приложения
// TODO: Добавить сортировку default ну типо скип сортировки чтоб оно сортировалось никак а модет написать метод в engine посмотреть есть ли сортировки в исхолное состояние вернуть как то

using Process_manager.UiResources;
using Process_Manager.AppLoggeres;
using Process_Manager.Display.Engine;
using Process_Manager.Enums;
using System.Diagnostics;

namespace Process_Manager.Display;

internal class Display
{
    private const int CidTextSpaceLimit = 2;
    private const int NameTextSpaceLimit = 26;
    private const int PidTextSpaceLimit = 5;
    private const int MemoryTextSpaceLimit = 5;
    private const int CountProcessesInPage = 20;

    private readonly float _totalMemoryGb = (float)GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024 * 1024);
    private readonly Lock _locker = new();

    private bool _isListDisplayed = true;
    private int _countOfPages;
    private int _currentPage = 0;

    private Process[]? _page;
    private SortType _currentSortType = SortType.None;
    private ConsoleKeyInfo _consoleKey = default;

    private Process[] _processes = Process.GetProcesses();
    private CancellationTokenSource? _ctsDisplayList;
    private CancellationTokenSource? _ctsUpdateDataList;

    public void MainMenu()
    {
        AppLogger.Log("// - admin comments");
        AppLogger.Log("");

        while (true)
        {
            AppLogger.Log("Draw main menu");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(UiResource.Logo);
            Console.ResetColor();

            for (int i = 0; i < UiResource.MenuOptions.Length; i++)
            {
                int leftPartLength = 0;
                int xPositionCursor = 75;
                int yPositionCursor = 12;

                Console.SetCursorPosition(xPositionCursor, yPositionCursor + i);

                for (int j = 0; j < UiResource.MenuOptions[i].Length; j++)
                {
                    if (UiResource.MenuOptions[i][j] == ':')
                    {
                        leftPartLength = j;
                    }
                }

                for (int l = 0; l < leftPartLength; l++)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(UiResource.MenuOptions[i][l]);
                }

                Console.ResetColor();

                for (int k = leftPartLength; k < UiResource.MenuOptions[i].Length; k++)
                {
                    Console.Write(UiResource.MenuOptions[i][k]);
                }
            }

            AppLogger.Log("Get user input");
            ConsoleKeyInfo consoleKey = DisplayEngine.GetHiddenUserInput();

            switch (consoleKey.Key)
            {
                case ConsoleKey.Enter:
                    AppLogger.Log("Start async tasks");

                    _ctsUpdateDataList = new();
                    _ctsDisplayList = new();

                    _ = UpdateProcessesAsync(_ctsUpdateDataList);
                    _ = DisplayProcessesAsync(_ctsDisplayList);

                    MainDisplay();
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
        while (true)
        {
            AppLogger.Log("Dispaly list");

            Console.Clear();
            EnableDisplayList();

            AppLogger.Log("Get user input");

            _consoleKey = DisplayEngine.GetHiddenUserInput();
            switch (_consoleKey.Key)
            {
                case ConsoleKey.E:
                    AppLogger.Log("User choice: 'next page'");

                    if (_currentPage < _countOfPages) _currentPage++;
                    continue;

                case ConsoleKey.Q:
                    AppLogger.Log("User choice: 'privous page'");

                    if (_currentPage > 0) _currentPage--;
                    continue;

                case ConsoleKey.Oem3:
                    AppLogger.Log("User choice: 'ProcessManage'");

                    ManageProcess();
                    continue;

                case ConsoleKey.Tab:
                    AppLogger.Log("User choice: 'ProcessFilter'");

                    FilterProcesses();
                    continue;

                case ConsoleKey.F1:
                    AppLogger.Log("User choice: 'Search Page'");

                    SearchPage();
                    continue;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    AppLogger.Log("User choice: 'exit'");

                    _ctsUpdateDataList?.Cancel();
                    _ctsDisplayList?.Cancel();
                    return;

                default:
                    AppLogger.Log("Error: 'wrong input'");

                    DisableDisplayList();
                    DisplayError(ErrorType.Wrong_Input);
                    continue;
            }
        }

        bool FilterProcesses()
        {
            while (true)
            {
                DisableDisplayList();

                Console.ResetColor();
                Console.WriteLine();

                AppLogger.Log("FILTER: Draw filter options");

                for (int i = 0; i < UiResource.FilterOptions.Length; i++)
                {
                    Console.WriteLine(UiResource.FilterOptions[i]);
                }

                AppLogger.Log("FILTER: Get user input");

                _consoleKey = DisplayEngine.GetHiddenUserInput();
                switch (_consoleKey.Key)
                {
                    case ConsoleKey.D1:
                        AppLogger.Log("FILTER: User choice: 'filter by name'");

                        _currentSortType = SortType.Name;
                        DisplayEngine.SortProcessesByName(_processes);
                        return true;

                    case ConsoleKey.D2:
                        AppLogger.Log("FILTER: User choice: 'filter by PID");

                        _currentSortType = SortType.PID;
                        DisplayEngine.SortProcessesByPid(_processes);
                        return true;

                    case ConsoleKey.D3:
                        AppLogger.Log("FILTER: User choice: 'filter by Memory'");

                        _currentSortType = SortType.Memory;
                        DisplayEngine.SortProcessesByMemory(_processes);
                        return true;

                    case ConsoleKey.Backspace:
                    case ConsoleKey.Escape:
                        AppLogger.Log("FILTER: User choice: 'exit'");
                        return false;

                    default:
                        AppLogger.Log("FILTER: Error: 'wrong input'");

                        DisplayError(ErrorType.Wrong_Input);
                        EnableDisplayList();
                        continue;
                }
            }
        }

        bool ManageProcess()
        {
            while (true)
            {
                DisableDisplayList();

                Console.ResetColor();
                Console.Write($"\nEnter a CID: ");

                string? userIndexString = Console.ReadLine();

                if (!DisplayEngine.InitNumber(userIndexString ?? String.Empty, out int userIndex))
                {
                    DisplayError(ErrorType.Wrong_Input);
                    EnableDisplayList();
                    continue;
                }

                if (userIndex < 0 || userIndex > _processes.Length - 1) // TODO MARK
                {
                    DisplayError(ErrorType.Wrong_Input);
                    EnableDisplayList();
                    continue;
                }

                AppLogger.Log("MANAGER: Draw options");

                for (int i = 0; i < UiResource.ProcessOptions.Length; i++)
                {
                    Console.WriteLine(UiResource.ProcessOptions[i]);
                }

                Console.Write($"\nChoose option\n");

                ConsoleKeyInfo consoleKey = DisplayEngine.GetHiddenUserInput();
                switch (consoleKey.Key)
                {
                    case ConsoleKey.D1:
                        AppLogger.Log("MANAGER: User choice: 'kill process'");

                        if (!DisplayEngine.KillProcess(_processes, userIndex))
                            DisplayError(ErrorType.Run_As_Administator);
                        return true;

                    case ConsoleKey.D2:
                        AppLogger.Log("MANAGER: User choice: 'soft kill'");

                        if (!DisplayEngine.CloseMainWindowProcess(_processes, userIndex))
                            DisplayError(ErrorType.Run_As_Administator);
                        return true;

                    case ConsoleKey.D3:
                        AppLogger.Log("MANAGER: User choice: 'Open file directory'");

                        if (!DisplayEngine.OpenFileDirectoryProcess(_processes, userIndex))
                            DisplayError(ErrorType.Run_As_Administator);
                        return true;

                    case ConsoleKey.D4:
                        AppLogger.Log("MANAGER: User choice: 'change priority'");

                        ChangePriority(userIndex);
                        return true;

                    case ConsoleKey.D9:
                        AppLogger.Log("MANAGER: User CHECK POINT");
                        return true;

                    case ConsoleKey.Backspace:
                    case ConsoleKey.Escape:
                        AppLogger.Log("MANAGER: User choice: 'exit'");
                        return false;

                    default:
                        AppLogger.Log("FILTER: Error: 'wrong input'");

                        DisplayError(ErrorType.Wrong_Input);
                        EnableDisplayList();
                        continue;
                }
            }
        }


        bool ChangePriority(int userIndex)
        {
            while (true)
            {
                AppLogger.Log("Draw options");

                for (int i = 0; i < UiResource.ChangePriorityOptions.Length; i++)
                {
                    Console.WriteLine(UiResource.ChangePriorityOptions[i]);
                }

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

                    case ConsoleKey.Backspace:
                    case ConsoleKey.Escape:
                        AppLogger.Log("User choice: 'exit'");
                        return false;

                    default:
                        AppLogger.Log("Error: 'wrong input'");
                        DisplayError(ErrorType.Wrong_Input);
                        return false;
                }
            }
        }


        bool SearchPage()
        {
            while (true)
            {
                DisableDisplayList();

                Console.ResetColor();
                Console.Write($"\nEnter a number of page: ");

                string? userIndexString = Console.ReadLine();

                if (!DisplayEngine.InitNumber(userIndexString ?? String.Empty, out int userIndex))
                {
                    DisplayError(ErrorType.Wrong_Input);
                    EnableDisplayList();
                    continue;
                }

                if (userIndex < 0 || userIndex > _countOfPages)
                {
                    DisplayError(ErrorType.Wrong_Input);
                    EnableDisplayList();
                    continue;
                }

                _currentPage = userIndex;
                return false;
            }
        }
    }

    private async Task DisplayProcessesAsync(CancellationTokenSource tokenSource)
    {
        while (!tokenSource.Token.IsCancellationRequested)
        {
            if (_isListDisplayed == true)
            {
                AppLogger.Log("ASYNC: if approved");

                var currentProcesses = _processes;
                float totalMemoryUsage = 0;

                _countOfPages = _processes.Length / CountProcessesInPage;

                _page = [ ..currentProcesses
                .Skip(CountProcessesInPage * _currentPage)
                .Take(CountProcessesInPage) ];

                if (currentProcesses == null || currentProcesses.Length == 0)
                {
                    AppLogger.Log("ASYNC: null array");
                    AppLogger.Log("ASYNC: await 50 ms");

                    await Task.Delay(50);
                    continue;
                }

                AppLogger.Log("ASYNC: lock display");

                lock (_locker)
                {
                    Console.SetCursorPosition(0, 0);

                    AppLogger.Log("ASYNC: Draw header");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("'Q' left | 'E' right | 'F1' search page | 'TAB' filter | '`' manage | 'ESC / BACKSPACE' return");
                    Console.WriteLine($"Current page: {_currentPage}|{_countOfPages}      \n\n");

                    AppLogger.Log("ASYNC: Draw global stats");

                    for (int i = 0; i < currentProcesses.Length; i++)
                    {
                        totalMemoryUsage += (float)currentProcesses[i].PrivateMemorySize64 / (1024 * 1024);

                        if (i == currentProcesses.Length - 1)
                        {
                            Console.WriteLine($"Total memory usage: {totalMemoryUsage,4} / {_totalMemoryGb} MB | Count of processes: {currentProcesses.Length}     ");
                        }
                    }

                    AppLogger.Log("ASYNC: Draw processes");

                    for (int i = 0; i < _page.Length; i++)
                    {
                        ConsoleColor currentColor;
                        string moduleFullNamePath = DisplayEngine.GetProcessModuleFullName(_page[i]);
                        string nameExtension = Path.GetExtension(moduleFullNamePath);
                        string processName = _page[i].ProcessName;
                        float memoryUsage = _page[i].PrivateMemorySize64 / (1024 * 1024);

                        if (i % 2 == 0)
                        {
                            currentColor = ConsoleColor.DarkGray;
                        }

                        else
                        {
                            currentColor = ConsoleColor.Gray;
                        }

                        if (_page[i].ProcessName.Length >= 25)
                        {
                            processName = _page[i].ProcessName[..22] + "..." + nameExtension;
                        }
                        else
                        {
                            processName += nameExtension;
                        }

                        Console.Write($"| CID: {currentProcesses.IndexOf(_page[i]),-CidTextSpaceLimit} \t", Console.ForegroundColor = currentColor);
                        Console.Write($"| Name: {processName,-NameTextSpaceLimit}\t", Console.ForegroundColor = ConsoleColor.Yellow);
                        Console.Write($"| PID: {_page[i].Id,-PidTextSpaceLimit} \t", Console.ForegroundColor = currentColor);
                        Console.Write($"| Memory: {memoryUsage,-MemoryTextSpaceLimit} MB     \n", Console.ForegroundColor = ConsoleColor.Green);
                    }
                }
            }
            AppLogger.Log("ASYNC: await 950 ms");

            await Task.Delay(950);
        }
    }

    private async Task UpdateProcessesAsync(CancellationTokenSource tokenSource) // TODO: перенос логики в Display Engine || На подумать!
    {
        while (!tokenSource.IsCancellationRequested)
        {
            AppLogger.Log("ASYNC: Get processes");

            _processes = Process.GetProcesses();

            switch (_currentSortType)
            {
                case SortType.Name:
                    AppLogger.Log("ASYNC: Sort by name");

                    DisplayEngine.SortProcessesByName(_processes);
                    break;

                case SortType.PID:
                    AppLogger.Log("ASYNC: Sort be processor");

                    DisplayEngine.SortProcessesByPid(_processes);
                    break;

                case SortType.Memory:
                    AppLogger.Log("ASYNC: Sort by memory");

                    DisplayEngine.SortProcessesByMemory(_processes);
                    break;
            }

            AppLogger.Log("ASYNC: await 800 ms");
            await Task.Delay(800);
        }
    }

    private static void DisplayError(ErrorType errorType)
    {

        AppLogger.Log($"Error: {errorType}");

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;

        switch (errorType)
        {
            case ErrorType.Run_As_Administator:
                AppLogger.Log("try run as admin");

                Console.WriteLine("Error #1: Try to run the program as administrator");
                break;

            case ErrorType.Wrong_Input:
                AppLogger.Log("Wrong input");

                Console.WriteLine("Error #2: Wrong input, make sure you have entered it correctly.");
                break;

            default:
                AppLogger.Log("Error 404");

                Console.WriteLine("Error #404: Unknown error");
                break;
        }

        Console.ResetColor();
        DisplayEngine.BlockInputInThreadSleep(1500);
        Console.Clear();
    }

    private void EnableDisplayList()
    {
        _isListDisplayed = true;
        DisplayEngine.BlockInputInThreadSleep(800);
    }

    private void DisableDisplayList()
    {
        _isListDisplayed = false;
    }
}