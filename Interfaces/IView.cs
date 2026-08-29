using ProcessManager.Enums.ErrorTypes;
using ProcessManager.Structs;
using System.Diagnostics;
using System.Net;

namespace ProcessManager.Interfaces.Iviews;

internal interface IView
{
    event Action OnMenuClicked;
    event Action OnSearchPageClicked;
    event Action OnMainDisplayClicked;
    event Action OnManageProcessClicked;
    event Action OnFilterProcessesClicked;
    event Action<int> OnChangePriorityClicked;

    void DrawPage(List<ProcessStruct> processes);
    void DrawStats(float totalMemoryUsage, float totalMemoryGb, int countOfProcesses);
    void DrawHeader(int currentPage, int countOfPages);
    void DisplayError(ErrorType errorType);
    void ChangePriority(int userIndex);

    void ChangePriorityOptionDraw();
    void FilterMemoryOptionsDraw();
    void EnterNumberOfPage();
    void ManageOptionDraw();
    void FilterProcesses();
    void ManageProcess();
    void MainMenuDraw();
    void CursorToTop();
    void MainDisplay();
    void SearchPage();
    void ResetColor();
    void ClearText();
    void MainMenu();
    void EnterCid();
}