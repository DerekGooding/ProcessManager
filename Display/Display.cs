// TODO: Сделать рефактор логики.
// TODO: Сделать новые логи, точнее проверить старые, может внести конкретику и где то что то добавить.
// TODO: Выпустить релиз приложения с log vers и без log просто vers
// TODO: Написать README для приложения
// TODO: Последний текст ну на послежней страницу если длина нового списка на послденей странице меньше прошло то старый элемент не удалиться  ( его не перепишет )
// TODO: Block mouse in console ( later )

using ProcessManager.Displays.Engine.DisplayHelpers;
using ProcessManager.Displays.Engine.ConsoleHelpers;
using ProcessManager.Displays.Engine.NativeMethodes;
using ProcessManager.UiResources;
using ProcessManager.AppLoggeres;
using ProcessManager.ErrorTypes;
using ProcessManager.SortTypes;
using System.Diagnostics;

namespace ProcessManager.Displays;

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
            Console.Clear();
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
            ConsoleKeyInfo consoleKey = ConsoleHelper.GetHiddenUserInput();

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

                    DisplayHelper.ExitProgram();
                    break;

                default:
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

            _consoleKey = ConsoleHelper.GetHiddenUserInput();
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
                    DisableDisplayList();
                    DisplayError(ErrorType.Wrong_Input);
                    continue;
            }
        }
    }

    private void ManageProcess()
    {
        while (true)
        {
            DisableDisplayList();

            Console.ResetColor();
            Console.Write($"\nEnter a CID: ");

            string userIndexString = Console.ReadLine() ?? String.Empty;

            if (!DisplayHelper.IsNumber(userIndexString, out int userIndex))
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

            ConsoleKeyInfo consoleKey = ConsoleHelper.GetHiddenUserInput();
            switch (consoleKey.Key)
            {
                case ConsoleKey.D1:
                    AppLogger.Log("MANAGER: User choice: 'kill process'");

                    if (!DisplayHelper.KillProcess(_processes, userIndex))
                        DisplayError(ErrorType.Run_As_Administator);
                    return;

                case ConsoleKey.D2:
                    AppLogger.Log("MANAGER: User choice: 'soft kill'");

                    if (!DisplayHelper.CloseMainWindowProcess(_processes, userIndex))
                        DisplayError(ErrorType.Run_As_Administator);
                    return;

                case ConsoleKey.D3:
                    AppLogger.Log("MANAGER: User choice: 'Open file directory'");

                    if (!DisplayHelper.OpenFileDirectoryProcess(_processes, userIndex))
                        DisplayError(ErrorType.Run_As_Administator);
                    return;

                case ConsoleKey.D4:
                    AppLogger.Log("MANAGER: User choice: 'change priority'");

                    ChangePriority(userIndex);
                    return;

                case ConsoleKey.D9:
                    AppLogger.Log("MANAGER: User CHECK POINT");
                    return;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    AppLogger.Log("MANAGER: User choice: 'exit'");
                    return;

                default:
                    DisplayError(ErrorType.Wrong_Input);
                    EnableDisplayList();
                    continue;
            }
        }
    }

    private void ChangePriority(int userIndex)
    {
        while (true)
        {
            AppLogger.Log("Draw options");

            for (int i = 0; i < UiResource.ChangePriorityOptions.Length; i++)
            {
                Console.WriteLine(UiResource.ChangePriorityOptions[i]);
            }

            AppLogger.Log("Get user input");

            ConsoleKeyInfo consoleKey = ConsoleHelper.GetHiddenUserInput();

            switch (consoleKey.Key)
            {
                case ConsoleKey.D1:
                    AppLogger.Log("User choice: 'change priority RealTime'");

                    DisplayHelper.ChangePriorityProcess(_processes, userIndex, ProcessPriorityClass.RealTime);
                    return;

                case ConsoleKey.D2:
                    AppLogger.Log("User choice: 'change priority RealTime'");

                    DisplayHelper.ChangePriorityProcess(_processes, userIndex, ProcessPriorityClass.High);
                    return;

                case ConsoleKey.D3:
                    AppLogger.Log("User choice: 'change priority RealTime'");

                    DisplayHelper.ChangePriorityProcess(_processes, userIndex, ProcessPriorityClass.AboveNormal);
                    return;

                case ConsoleKey.D4:
                    AppLogger.Log("User choice: 'change priority RealTime'");

                    DisplayHelper.ChangePriorityProcess(_processes, userIndex, ProcessPriorityClass.Normal);
                    return;

                case ConsoleKey.D5:
                    AppLogger.Log("User choice: 'change priority RealTime'");

                    DisplayHelper.ChangePriorityProcess(_processes, userIndex, ProcessPriorityClass.BelowNormal);
                    return;

                case ConsoleKey.D6:
                    AppLogger.Log("User choice: 'change priority RealTime'");

                    DisplayHelper.ChangePriorityProcess(_processes, userIndex, ProcessPriorityClass.Idle);
                    return;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    AppLogger.Log("User choice: 'exit'");
                    return;

                default:
                    DisplayError(ErrorType.Wrong_Input);
                    EnableDisplayList();
                    continue;
            }
        }
    }

    private void SearchPage()
    {
        while (true)
        {
            DisableDisplayList();

            Console.ResetColor();
            Console.Write($"\nEnter a number of page: ");

            string? userIndexString = Console.ReadLine();

            if (!DisplayHelper.IsNumber(userIndexString ?? String.Empty, out int userIndex))
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
            return;
        }
    }

    private void FilterProcesses()
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

            _consoleKey = ConsoleHelper.GetHiddenUserInput();
            switch (_consoleKey.Key)
            {
                case ConsoleKey.D1:
                    AppLogger.Log("FILTER: User choice: 'filter by name'");

                    _currentSortType = SortType.Name;
                    DisplayHelper.SortProcessesByName(_processes);
                    return;

                case ConsoleKey.D2:
                    AppLogger.Log("FILTER: User choice: 'filter by PID");

                    _currentSortType = SortType.PID;
                    DisplayHelper.SortProcessesByPid(_processes);
                    return;

                case ConsoleKey.D3:
                    AppLogger.Log("FILTER: User choice: 'filter by Memory'");

                    _currentSortType = SortType.Memory;
                    DisplayHelper.SortProcessesByMemory(_processes);
                    return;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    AppLogger.Log("FILTER: User choice: 'exit'");
                    return;

                default:
                    DisplayError(ErrorType.Wrong_Input);
                    EnableDisplayList();
                    continue;
            }
        }
    }

    private async Task DisplayProcessesAsync(CancellationTokenSource tokenSource)
    {
        while (!tokenSource.Token.IsCancellationRequested)
        {
            if (_isListDisplayed)
            {
                AppLogger.Log("ASYNC: if approved");

                var currentProcesses = _processes;
                float totalMemoryUsage = 0;

                _countOfPages = _processes.Length / CountProcessesInPage;

                _page = [ ..currentProcesses
                .Skip(CountProcessesInPage * _currentPage)
                .Take(CountProcessesInPage) ]; // TODO: Чекнуть можно ли куда перенести

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
                        string moduleFullNamePath = NativeMethod.GetProcessModuleFullName(_page[i]);
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

            await Task.Delay(950); // TODO: Check ignore tokel cancel by task delay
        }
    }

    private async Task UpdateProcessesAsync(CancellationTokenSource tokenSource) // TODO: Разнциа между token source и просто token
    {
        while (!tokenSource.IsCancellationRequested)
        {
            AppLogger.Log("ASYNC: Get processes");

            _processes = Process.GetProcesses();

            switch (_currentSortType)
            {
                case SortType.Name:
                    AppLogger.Log("ASYNC: Sort by name");

                    DisplayHelper.SortProcessesByName(_processes);
                    break;

                case SortType.PID:
                    AppLogger.Log("ASYNC: Sort be processor");

                    DisplayHelper.SortProcessesByPid(_processes);
                    break;

                case SortType.Memory:
                    AppLogger.Log("ASYNC: Sort by memory");

                    DisplayHelper.SortProcessesByMemory(_processes);
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
        ConsoleHelper.BlockInputInThreadSleep(1500);
        Console.Clear();
    }

    private void EnableDisplayList()
    {
        _isListDisplayed = true;
        ConsoleHelper.BlockInputInThreadSleep(1060);
    }

    private void DisableDisplayList()
    {
        _isListDisplayed = false;
    }
}