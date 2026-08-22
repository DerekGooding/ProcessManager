// TODO: Сделать рефактор логики.
// TODO: Сделать новые логи, точнее проверить старые, может внести конкретику и где то что то добавить.
// TODO: Namespace подправить если что

using Process_manager.Engine;
using Process_manager.Interfaces;
using ProcessManager.AppLoggeres;
using ProcessManager.ErrorTypes;
using ProcessManager.UiResources;
using System.Diagnostics;

namespace ProcessManager.Displays;

internal class Display : IView
{
    public event Action? AsyncDisplayListPageHandler;
    public event Action? AsyncDisplayListHeaderHandler;

    public event Action? OnMenuClicked;
    public event Action? OnMainDisplayClicked;
    public event Action? OnManageProcessClicked;
    public event Action? OnFilterProcessesClicked;
    public event Action? OnSearchPageClicked;
    public event Action<int>? OnChangePriorityClicked;

    private const int CidTextSpaceLimit = 2;
    private const int NameTextSpaceLimit = 26;
    private const int PidTextSpaceLimit = 5;
    private const int MemoryTextSpaceLimit = 5;
    private const int TotalMemoryUsageTextSpaceLimit = 4;

    private readonly Lock _locker = new();

    public void MainMenu()
    {
        AppLogger.Log("Draw main menu");

        Console.Clear();

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
        OnMenuClicked?.Invoke();

    }

    public void MainDisplay()
    {
        OnMainDisplayClicked?.Invoke();
    }

    public void ManageProcess()
    {

        OnManageProcessClicked?.Invoke();

        for (int i = 0; i < UiResource.ProcessOptions.Length; i++)
        {
            Console.WriteLine(UiResource.ProcessOptions[i]);
        }

    }

    public void ChangePriority(int userIndex)
    {

        OnChangePriorityClicked?.Invoke(userIndex);

        for (int i = 0; i < UiResource.ChangePriorityOptions.Length; i++)
        {
            Console.WriteLine(UiResource.ChangePriorityOptions[i]);
        }

    }

    public void SearchPage()
    {
        OnSearchPageClicked?.Invoke();
    }

    public void FilterProcesses()
    {
        OnFilterProcessesClicked?.Invoke();

        Console.WriteLine();
        for (int i = 0; i < UiResource.FilterOptions.Length; i++)
        {
            Console.WriteLine(UiResource.FilterOptions[i]);
        }
    }

    public async Task DisplayProcessesAsync(CancellationToken token, Process[] page, AutoResetEvent autoResetEvent)
    {
        while (!token.IsCancellationRequested)
        {
            AppLogger.Log("DISPLAY ASYNC: Start method");

            autoResetEvent.WaitOne();

            AsyncDisplayListPageHandler?.Invoke();

            lock (_locker)
            {
                AppLogger.Log("DISPLAY ASYNC: in lock");
                Console.SetCursorPosition(0, 0);

                Console.ForegroundColor = ConsoleColor.Gray;
                
                AsyncDisplayListHeaderHandler?.Invoke();

                for (int i = 0; i < page.Length; i++)
                {
                    ConsoleColor currentColor;
                    string moduleFullNamePath = NativeProcessService.GetProcessModuleFullName(page[i]);
                    string nameExtension = Path.GetExtension(moduleFullNamePath);
                    string processName = page[i].ProcessName;
                    float memoryUsage = page[i].PrivateMemorySize64 / (1024 * 1024);

                    if (i % 2 == 0)
                        currentColor = ConsoleColor.DarkGray;
                    else
                        currentColor = ConsoleColor.Gray;

                    if (page[i].ProcessName.Length >= 25)
                        processName = page[i].ProcessName[..22] + "..." + nameExtension;
                    else
                        processName += nameExtension;

                    Console.Write($"| CID: {page.IndexOf(page[i]),-CidTextSpaceLimit} \t", Console.ForegroundColor = currentColor); // сделать для cid массив full process'ов 
                    Console.Write($"| Name: {processName,-NameTextSpaceLimit}\t", Console.ForegroundColor = ConsoleColor.Yellow);
                    Console.Write($"| PID: {page[i].Id,-PidTextSpaceLimit} \t", Console.ForegroundColor = currentColor);
                    Console.Write($"| Memory: {memoryUsage,-MemoryTextSpaceLimit} MB     \n", Console.ForegroundColor = ConsoleColor.Green);
                }
            }
            AppLogger.Log("DISPLAY ASYNC: await 950ms");
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
        Console.Clear();
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
}