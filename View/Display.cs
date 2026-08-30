using ProcessManager.Enums.ErrorTypes;
using ProcessManager.Enums.ProcessChangePriorityTypes;
using ProcessManager.Enums.ProcessManageTypes;
using ProcessManager.Enums.SortTypes;
using ProcessManager.Enums.VirtualKeyTypes;
using ProcessManager.Interfaces.Iviews;
using ProcessManager.Models.InputServices;
using ProcessManager.Models.NativeConsoleMethods;
using ProcessManager.Structs;
using ProcessManager.UiResources;

namespace ProcessManager.Displays;

internal class Display : IView
{
    public event Action<SortType>? OnProcessesFilterOptionRequested;
    public event Action<ProcessManageType, int>? OnManageOptionRequested;
    public event Action<ProcessChangePriorityType, int>? OnChangePriorityOptionRequested;

    public event Action<int>? OnSearchPageCheckValue;
    public event Action<int>? OnManageProcessCheckCidValue;

    public event Action? OnDefaultMainMenuRequested;
    public event Action? OnDefaultMainDisplayRequested;
    public event Action<ErrorType>? OnDefaultGeneralRequested;

    public event Action? OnChangePriorityReady;
    public event Action? OnSearchPageReady;
    public event Action? OnFilterProcessesReady;
    public event Action? OnManageProcessReady;
    public event Action? OnMainDisplayReady;

    public event Action? OnEnterRequested;
    public event Action? OnExitRequested;
    public event Action? OnReturnRequested;

    public event Action? OnPreviousPageRequested;
    public event Action? OnNextPageRequested;

    private const int TotalMemoryUsageTextSpaceLimit = 4;
    private const int MemoryTextSpaceLimit = 5;
    private const int XPositionCursorLogo = 75;
    private const int YPositionCursorLogo = 12;
    private const int NameTextSpaceLimit = 31;
    private const int PidTextSpaceLimit = 5;

    private int _leftPartLengthLogo = 0;
    private ConsoleColor consoleColor = ConsoleColor.White;

    public void MainMenu()
    {
        while (true)
        {
            MainMenuDraw();

            switch (NativeConsoleMethod.GetHiddenUserInput())
            {
                case VirtualKeyType.VK_RETURN:
                    OnEnterRequested?.Invoke();
                    MainDisplay();
                    break;

                case VirtualKeyType.VK_BACK:
                case VirtualKeyType.VK_ESCAPE:
                    OnExitRequested?.Invoke();
                    break;

                default:
                    OnDefaultMainMenuRequested?.Invoke();
                    continue;
            }
        }
    }

    public void MainDisplay()
    {
        while (true)
        {
            OnMainDisplayReady?.Invoke();

            switch (NativeConsoleMethod.GetHiddenUserInput())
            {
                case VirtualKeyType.VK_E:
                    OnNextPageRequested?.Invoke();
                    continue;

                case VirtualKeyType.VK_Q:
                    OnPreviousPageRequested?.Invoke();
                    continue;

                case VirtualKeyType.VK_OEM_3:
                    ManageProcess();
                    continue;

                case VirtualKeyType.VK_TAB:
                    FilterProcesses();
                    continue;

                case VirtualKeyType.VK_F1:
                    SearchPage();
                    continue;

                case VirtualKeyType.VK_BACK:
                case VirtualKeyType.VK_ESCAPE:
                    OnReturnRequested?.Invoke();
                    return;

                default:
                    OnDefaultMainDisplayRequested?.Invoke();
                    continue;
            }
        }
    }

    public void ManageProcess()
    {
        OnManageProcessReady?.Invoke();

        EnterCid();

        if (!int.TryParse(Console.ReadLine(), out int userIndex))
        {
            OnDefaultGeneralRequested?.Invoke(ErrorType.Wrong_Input);
            return;
        }

        OnManageProcessCheckCidValue?.Invoke(userIndex);

        switch (NativeConsoleMethod.GetHiddenUserInput())
        {
            case VirtualKeyType.VK_1:
                OnManageOptionRequested?.Invoke(ProcessManageType.KillProcess, userIndex);
                return;

            case VirtualKeyType.VK_2:
                OnManageOptionRequested?.Invoke(ProcessManageType.CloseProcess, userIndex);
                return;

            case VirtualKeyType.VK_3:
                OnManageOptionRequested?.Invoke(ProcessManageType.OpenFileDirectory, userIndex);
                return;

            case VirtualKeyType.VK_4:
                ChangePriority(userIndex);
                return;

            case VirtualKeyType.VK_BACK:
            case VirtualKeyType.VK_ESCAPE:
                return;

            default:
                OnDefaultGeneralRequested?.Invoke(ErrorType.Wrong_Input);
                return;
        }
    }

    public void SearchPage()
    {
        OnSearchPageReady?.Invoke();

        if (!int.TryParse(InputService.GetUserMultiInput(), out int userIndex))
        {
            OnDefaultGeneralRequested?.Invoke(ErrorType.Wrong_Input);
            return;
        }

        OnSearchPageCheckValue?.Invoke(userIndex);
    }

    public void FilterProcesses()
    {
        OnFilterProcessesReady?.Invoke();

        switch (NativeConsoleMethod.GetHiddenUserInput())
        {
            case VirtualKeyType.VK_1:
                OnProcessesFilterOptionRequested?.Invoke(SortType.Name);
                return;

            case VirtualKeyType.VK_2:
                OnProcessesFilterOptionRequested?.Invoke(SortType.Pid);
                return;

            case VirtualKeyType.VK_3:
                OnProcessesFilterOptionRequested?.Invoke(SortType.Memory);
                return;

            case VirtualKeyType.VK_BACK:
            case VirtualKeyType.VK_ESCAPE:
                return;

            default:
                OnDefaultGeneralRequested?.Invoke(ErrorType.Wrong_Input);
                return;
        }
    }

    public void ChangePriority(int userIndex)
    {
        OnChangePriorityReady?.Invoke();

        switch (OnWaitingUserInput?.Invoke)
        {
            case VirtualKeyType.VK_1:
                OnChangePriorityOptionRequested?.Invoke(ProcessChangePriorityType.RealTime, userIndex);
                return;

            case VirtualKeyType.VK_2:
                OnChangePriorityOptionRequested?.Invoke(ProcessChangePriorityType.High, userIndex);
                return;

            case VirtualKeyType.VK_3:
                OnChangePriorityOptionRequested?.Invoke(ProcessChangePriorityType.AboveNormal, userIndex);
                return;

            case VirtualKeyType.VK_4:
                OnChangePriorityOptionRequested?.Invoke(ProcessChangePriorityType.Normal, userIndex);
                return;

            case VirtualKeyType.VK_5:
                OnChangePriorityOptionRequested?.Invoke(ProcessChangePriorityType.BelowNormal, userIndex);
                return;

            case VirtualKeyType.VK_6:
                OnChangePriorityOptionRequested?.Invoke(ProcessChangePriorityType.Idle, userIndex);
                return;

            case VirtualKeyType.VK_BACK:
            case VirtualKeyType.VK_ESCAPE:
                return;

            default:
                OnDefaultGeneralRequested?.Invoke(ErrorType.Wrong_Input);
                break;
        }
    }

    public void DisplayError(ErrorType errorType)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;

        switch (errorType)
        {
            case ErrorType.Run_As_Administator:
                Console.WriteLine("Error #1: Try to run the program as administrator");
                break;

            case ErrorType.Wrong_Input:
                Console.WriteLine("Error #2: Wrong input, make sure you have entered it correctly.");
                break;

            default:
                Console.WriteLine("Error #404: Unknown error");
                break;
        }
        Console.ResetColor();
    }

    public void EnterCid()
    {
        Console.ResetColor();
        Console.Write($"\nEnter a CID: ");
    }

    public void EnterNumberOfPage()
    {
        Console.ResetColor();
        Console.Write($"\nEnter a number of page: ");
    }

    public void DrawHeader(int currentPage, int countOfPages)
    {
        Console.WriteLine("'Q' left | 'E' right | 'F1' search page | 'TAB' filter | '`' manage | 'ESC / BACKSPACE' return");
        Console.WriteLine($"Current page: {currentPage}|{countOfPages}      \n\n");
    }

    public void DrawStats(float totalMemoryUsage, float totalMemoryGb, int countOfProcesses)
    {
        Console.WriteLine($"Total memory usage: {totalMemoryUsage,TotalMemoryUsageTextSpaceLimit} / {totalMemoryGb} MB | Count of processes: {countOfProcesses}     ");
    }

    public void DrawPage(List<ProcessStruct> processes)
    {
        for (int i = 0; i < processes.Count; i++)
        {
            if (processes[i].process == null)
            {
                Console.WriteLine(UiResource.EmptyStroke);
            }
            else
            {
                if (processes[i].index % 2 == 0) consoleColor = ConsoleColor.DarkGray;
                else consoleColor = ConsoleColor.Gray;

                Console.Write($"| CID: {processes[i].index} \t", Console.ForegroundColor = consoleColor); // сделать для cid массив full process'ов 
                Console.Write($"| Name: {processes[i].processName,-NameTextSpaceLimit}\t", Console.ForegroundColor = ConsoleColor.Yellow);
                Console.Write($"| PID: {processes[i].process.Id,-PidTextSpaceLimit} \t", Console.ForegroundColor = consoleColor);
                Console.Write($"| Memory: {processes[i].memoryUsage,-MemoryTextSpaceLimit} MB     \n", Console.ForegroundColor = ConsoleColor.Green);
            }
        }
    }

    public void ManageOptionDraw()
    {
        Console.ResetColor();
        Console.WriteLine();

        for (int i = 0; i < UiResource.ProcessOptions.Length; i++)
        {
            Console.WriteLine(UiResource.ProcessOptions[i]);
        }
    }

    public void ChangePriorityOptionDraw()
    {
        Console.ResetColor();
        Console.WriteLine();

        for (int i = 0; i < UiResource.ChangePriorityOptions.Length; i++)
        {
            Console.WriteLine(UiResource.ChangePriorityOptions[i]);
        }
    }

    public void FilterMemoryOptionsDraw()
    {
        Console.ResetColor();
        Console.WriteLine();

        for (int i = 0; i < UiResource.FilterOptions.Length; i++)
        {
            Console.WriteLine(UiResource.FilterOptions[i]);
        }
    }

    public void MainMenuDraw()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(UiResource.Logo);
        Console.ResetColor();

        for (int i = 0; i < UiResource.MenuOptions.Length; i++)
        {
            Console.SetCursorPosition(XPositionCursorLogo, YPositionCursorLogo + i);

            for (int j = 0; j < UiResource.MenuOptions[i].Length; j++)
            {
                if (UiResource.MenuOptions[i][j] == ':')
                {
                    _leftPartLengthLogo = j;
                }
            }

            for (int l = 0; l < _leftPartLengthLogo; l++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(UiResource.MenuOptions[i][l]);
            }

            Console.ResetColor();

            for (int k = _leftPartLengthLogo; k < UiResource.MenuOptions[i].Length; k++)
            {
                Console.Write(UiResource.MenuOptions[i][k]);
            }
        }
    }

    public void ClearText() =>
        Console.Clear();

    public void CursorToTop() =>
        Console.SetCursorPosition(0, 0);

    public void ResetColor() =>
        Console.ResetColor();
}