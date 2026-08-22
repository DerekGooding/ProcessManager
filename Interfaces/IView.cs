using ProcessManager.ErrorTypes;
using System.Diagnostics;

namespace Process_manager.Interfaces;

internal interface IView
{
    event Action AsyncDisplayListPageHandler;
    event Action AsyncDisplayListHeaderHandler;

    event Action OnMenuClicked;
    event Action OnMainDisplayClicked;
    event Action OnManageProcessClicked;
    event Action OnFilterProcessesClicked;
    event Action OnSearchPageClicked;
    event Action<int> OnChangePriorityClicked;

    async Task DisplayProcessesAsync(CancellationToken tokenSource, Process[] processes, AutoResetEvent autoResetEvent) { }

    public void DisplayError(ErrorType errorType);
    void ChangePriority(int userIndex);
    void MainDisplay();
    void MainMenu();
    void FilterProcesses();
    void SearchPage();
    void ManageProcess();
    void EnterCid();
    void EnterNumberOfPage();
    void DrawHeader(int currentPage, int countOfPages);
    void DrawStats(float totalMemoryUsage, float totalMemoryGb, int countOfProcesses);
}