using ProcessManager.Enums.ErrorTypes;
using System.Diagnostics;

namespace ProcessManager.Interfaces.Iviews;

internal interface IView
{
    event Action AsyncDisplayListHeaderHandler;
    event Action<Process[]> AsyncDisplayPageLoadDataHandler;
    event Action<Process, ConsoleColor, int> AsyncDisplayProcessCheckDataHandler;
    event Action<int> OnChangePriorityClicked;

    event Action OnMenuClicked;
    event Action OnSearchPageClicked;
    event Action OnMainDisplayClicked;
    event Action OnManageProcessClicked;
    event Action OnFilterProcessesClicked;

    async Task DisplayProcessesAsync(Process[] processes, ManualResetEvent manualResetEvent, ConsoleColor currentColor, CancellationToken token) { }

    void DrawStats(float totalMemoryUsage, float totalMemoryGb, int countOfProcesses);
    void DrawProcess(Process process, ConsoleColor currentColor, int index);
    void DrawHeader(int currentPage, int countOfPages);
    void DisplayError(ErrorType errorType);
    void ChangePriority(int userIndex);

    void ChangePriorityOptionDraw();
    void FilterMemoryOptionsDraw();
    void EnterNumberOfPage();
    void ManageOptionDraw();
    void DrawEmptyStroke();
    void FilterProcesses();
    void ManageProcess();
    void MainMenuDraw();
    void MainDisplay();
    void SearchPage();
    void MainMenu();
    void EnterCid();
}