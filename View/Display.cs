using ProcessManager.Enums.ErrorTypes;
using ProcessManager.Interfaces.Iviews;
using ProcessManager.Models.NativeProcessServices;
using ProcessManager.UiResources;
using System.Diagnostics;

namespace ProcessManager.Displays;

internal class Display : IView
{
    public event Action<Process, ConsoleColor, int>? AsyncDisplayProcessCheckDataHandler;
    public event Action<Process[]>? AsyncDisplayPageLoadDataHandler;
    public event Action<int>? OnChangePriorityClicked;

    public event Action? AsyncDisplayListHeaderHandler;
    public event Action? OnFilterProcessesClicked;
    public event Action? OnManageProcessClicked;
    public event Action? OnMainDisplayClicked;
    public event Action? OnSearchPageClicked;
    public event Action? OnMenuClicked;

    private const int TotalMemoryUsageTextSpaceLimit = 4;
    private const int MemoryTextSpaceLimit = 5;
    private const int XPositionCursorLogo = 75;
    private const int YPositionCursorLogo = 12;
    private const int NameTextSpaceLimit = 31;
    private const int PidTextSpaceLimit = 5;

    private readonly Lock _locker = new();

    private int _leftPartLengthLogo = 0;

    public void MainMenu() =>
        OnMenuClicked?.Invoke();

    public void MainDisplay() =>
        OnMainDisplayClicked?.Invoke();

    public void ManageProcess() =>
        OnManageProcessClicked?.Invoke();

    public void ChangePriority(int userIndex) =>
        OnChangePriorityClicked?.Invoke(userIndex);

    public void SearchPage() =>
        OnSearchPageClicked?.Invoke();

    public void FilterProcesses() =>
        OnFilterProcessesClicked?.Invoke();

    public async Task DisplayProcessesAsync(Process[] page, ManualResetEvent manualResetEvent, ConsoleColor currentColor, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            manualResetEvent.WaitOne();

            lock (_locker)
            {
                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = ConsoleColor.Gray;

                AsyncDisplayPageLoadDataHandler?.Invoke(page);
                AsyncDisplayListHeaderHandler?.Invoke();

                for (int i = 0; i < page.Length; i++)
                {
                    if (i % 2 == 0) currentColor = ConsoleColor.DarkGray;
                    else currentColor = ConsoleColor.Gray;

                    AsyncDisplayProcessCheckDataHandler?.Invoke(page[i], currentColor, i);
                }
            }
            await Task.Delay(950, token);
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

    public void DrawEmptyStroke() =>
        Console.Write($"{UiResource.EmptyStroke}\n");

    public void DrawProcess(Process process, ConsoleColor currentColor, int index)
    {
        string moduleFullNamePath = NativeProcessService.GetProcessModuleFullName(process);
        string nameExtension = Path.GetExtension(moduleFullNamePath);
        string processName = process.ProcessName;
        float memoryUsage = process.PrivateMemorySize64 / (1024 * 1024);

        if (process.ProcessName.Length >= 25)
            processName = process.ProcessName[..22] + "..." + nameExtension;
        else
            processName += nameExtension;

        Console.Write($"| CID: {index} \t", Console.ForegroundColor = currentColor); // сделать для cid массив full process'ов 
        Console.Write($"| Name: {processName,-NameTextSpaceLimit}\t", Console.ForegroundColor = ConsoleColor.Yellow);
        Console.Write($"| PID: {process.Id,-PidTextSpaceLimit} \t", Console.ForegroundColor = currentColor);
        Console.Write($"| Memory: {memoryUsage,-MemoryTextSpaceLimit} MB     \n", Console.ForegroundColor = ConsoleColor.Green);
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
}