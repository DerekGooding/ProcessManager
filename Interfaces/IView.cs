using ProcessManager.Enums.ErrorTypes;
using ProcessManager.Enums.ProcessChangePriorityTypes;
using ProcessManager.Enums.ProcessManageTypes;
using ProcessManager.Enums.SortTypes;
using ProcessManager.Enums.VirtualKeyTypes;
using ProcessManager.Structs;
using System.Diagnostics;

namespace ProcessManager.Interfaces.Iviews;

internal interface IView
{
    event Func<VirtualKeyType> OnWaitingUserInput;

    event Action<SortType> OnProcessesFilterOptionRequested;
    event Action<ProcessManageType, int> OnManageOptionRequested;
    event Action<ProcessChangePriorityType, int> OnChangePriorityOptionRequested;

    event Action<int> OnManageProcessCheckCidValue;
    event Action<int> OnSearchPageCheckValue;

    event Action OnDefaultMainMenuRequested;
    event Action OnDefaultMainDisplayRequested;
    event Action<ErrorType> OnDefaultGeneralRequested;

    event Action OnChangePriorityReady;
    event Action OnSearchPageReady;
    event Action OnFilterProcessesReady;
    event Action OnManageProcessReady;
    event Action OnMainDisplayReady;

    event Action OnNextPageRequested;
    event Action OnPreviousPageRequested;

    event Action OnEnterRequested;
    event Action OnExitRequested;
    event Action OnReturnRequested;

    void DrawPage(List<ProcessStruct> processes);
    void DrawStats(float totalMemoryUsage, float totalMemoryGb, int countOfProcesses);
    void DrawHeader(int currentPage, int countOfPages);
    void DisplayError(ErrorType errorType);
    void ChangePriority(int userIndex);

    void ChangePriorityOptionDraw();
    void FilterMemoryOptionsDraw();
    void EnterNumberOfPage();
    void ManageOptionDraw();
    void MainMenuDraw();
    void CursorToTop();
    void MainDisplay();
    void ResetColor();
    void ClearText();
    void MainMenu();
    void EnterCid();
}