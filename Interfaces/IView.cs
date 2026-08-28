using ProcessManager.ErrorTypes;
using System.Diagnostics;

namespace Process_manager.Interfaces;

internal interface IView
{
    event Action AsyncDisplayListHeaderHandler;
    event Action<Process[]> AsyncDisplayPageLoadDataHandler;
    event Action<Process, ConsoleColor, int> AsyncDisplayProcessCheckDataHandler;

    event Action OnMenuClicked;
    event Action OnMainDisplayClicked;
    event Action OnManageProcessClicked;
    event Action OnFilterProcessesClicked;
    event Action OnSearchPageClicked;
    event Action<int> OnChangePriorityClicked;

    async Task DisplayProcessesAsync(CancellationToken token, Process[] processes, ManualResetEvent manualResetEvent, ConsoleColor currentColor) { }

    public void DisplayError(ErrorType errorType);
    void ChangePriority(int userIndex);
    void MainDisplay();
    void MainMenu();
    void FilterProcesses();
    void SearchPage();
    void ManageProcess();
    void EnterCid();
    void EnterNumberOfPage();
    void DrawEmptyStroke();
    void DrawHeader(int currentPage, int countOfPages);
    void DrawStats(float totalMemoryUsage, float totalMemoryGb, int countOfProcesses);
    void DrawProcess(Process process, ConsoleColor currentColor, int index);
    void ManageOptionDraw();
    void ChangePriorityOptionDraw();
    void FilterMemoryOptionsDraw();
    void MainMenuDraw();
}