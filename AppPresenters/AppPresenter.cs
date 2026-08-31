using System.Diagnostics;

namespace ProcessManager.AppPresenters;

internal class AppPresenter
{
    private const int CountProcessesInPage = 20;

    private readonly ManualResetEvent _manualResetEventPrepareDisplay = new(true);
    private readonly ManualResetEvent _manualResetEventPrepareData = new(true);
    private readonly float _totalMemoryGb = (float)GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024 * 1024);
    private readonly List<ProcessStruct> _processesList = [];
    private readonly IView _view;

    private CancellationTokenSource? _ctsDisplayList;
    private CancellationTokenSource? _ctsUpdateCountOfPage;
    private CancellationTokenSource? _ctsUpdateDataPage;
    private Process[]? _page;

    private SortType _sortType = SortType.None;
    private int _countOfPages;
    private int _currentPage = 0;

    private Process[] _processes = ProcessService.GetAllProcesses();

    public AppPresenter(IView view)
    {
        _view = view;

        view.OnProcessesFilterOptionRequested += FilterProcessesCheckOption;
        view.OnManageOptionRequested += ManageProcessCheckOption;
        view.OnChangePriorityOptionRequested += ChangePriorityProcessCheckOption;

        view.OnManageProcessCheckCidValue += OnManageProcessCheckCidValueMethod;
        view.OnSearchPageCheckValue += OnSearchPageCheckValue;

        view.OnDefaultGeneralRequested += ErrorHelper;
        view.OnDefaultMainMenuRequested += OnDefaultMainMenuClickedMethod;
        view.OnDefaultMainDisplayRequested += OnDefaultMainDisplayClickedMethod;

        view.OnMainDisplayReady += OnMainDisplayReadyMethod;
        view.OnManageProcessReady += OnManageProcessReadyMethod;
        view.OnChangePriorityReady += OnChangePriorityReady;
        view.OnFilterProcessesReady += OnFilterProcessesReady;
        view.OnSearchPageReady += OnSearchPageReady;

        view.OnNextPageRequested += OnNextPageRequestedMethod;
        view.OnPreviousPageRequested += OnPreviousPageRequestedMethod;

        view.OnEnterRequested += OnEnterClickedMethod;
        view.OnExitRequested += OnExitClickedMethod;
        view.OnReturnRequested += OnReturnRequested;
    }

    private void ChangePriorityProcessCheckOption(ProcessChangePriorityType processChangePriorityType, int userIndex)
    {
        if (_page is null)
            throw new InvalidOperationException("Process page is null.");

        switch (processChangePriorityType)
        {
            case ProcessChangePriorityType.RealTime:
                ProcessService.ChangePriorityProcess(_page, userIndex, ProcessPriorityClass.RealTime);
                return;

            case ProcessChangePriorityType.High:
                ProcessService.ChangePriorityProcess(_page, userIndex, ProcessPriorityClass.High);
                return;

            case ProcessChangePriorityType.AboveNormal:
                ProcessService.ChangePriorityProcess(_page, userIndex, ProcessPriorityClass.AboveNormal);
                return;

            case ProcessChangePriorityType.Normal:
                ProcessService.ChangePriorityProcess(_page, userIndex, ProcessPriorityClass.Normal);
                return;

            case ProcessChangePriorityType.BelowNormal:
                ProcessService.ChangePriorityProcess(_page, userIndex, ProcessPriorityClass.BelowNormal);
                return;

            case ProcessChangePriorityType.Idle:
                ProcessService.ChangePriorityProcess(_page, userIndex, ProcessPriorityClass.Idle);
                return;
        }
    }

    private void ManageProcessCheckOption(ProcessManageType processManageType, int userIndex)
    {
        if (_page is null)
            throw new InvalidOperationException("Process page is null.");

        switch (processManageType)
        {
            case ProcessManageType.KillProcess:
                if (!ProcessService.KillProcess(_page, userIndex))
                    ErrorHelper(ErrorType.Run_As_Administator);
                return;

            case ProcessManageType.CloseProcess:
                if (!ProcessService.CloseMainWindowProcess(_page, userIndex))
                    ErrorHelper(ErrorType.Run_As_Administator);
                return;

            case ProcessManageType.OpenFileDirectory:
                if (!ProcessService.OpenFileDirectoryProcess(_page, userIndex))
                    ErrorHelper(ErrorType.Run_As_Administator);
                return;
        }
    }

    private void FilterProcessesCheckOption(SortType sortType)
    {
        switch (sortType)
        {
            case SortType.Name:
                _sortType = SortType.Name;
                ProcessService.SortProcessesByName(_processes);
                return;

            case SortType.Pid:
                _sortType = SortType.Pid;
                ProcessService.SortProcessesByPid(_processes);
                return;

            case SortType.Memory:
                _sortType = SortType.Memory;
                ProcessService.SortProcessesByMemory(_processes);
                return;
        }
    }

    private void OnChangePriorityReady() => _view.ChangePriorityOptionDraw();

    private void OnManageProcessCheckCidValueMethod(int userIndex)
    {
        if (userIndex < 0 || userIndex > _page?.Length - 1)
            ErrorHelper(ErrorType.Wrong_Input);
        else
            _view.ManageOptionDraw();
    }

    private void OnManageProcessReadyMethod() => DisableDisplayList();

    private void OnNextPageRequestedMethod()
    {
        if (_currentPage < _countOfPages) _currentPage++;
    }

    private void OnPreviousPageRequestedMethod()
    {
        if (_currentPage > 0) _currentPage--;
    }

    private void OnMainDisplayReadyMethod()
    {
        _view.ClearText();
        EnableDisplayList();
    }

    private void OnReturnRequested()
    {
        _ctsUpdateCountOfPage?.Cancel();
        _ctsUpdateDataPage?.Cancel();
        _ctsDisplayList?.Cancel();

        _ctsUpdateCountOfPage?.Dispose();
        _ctsUpdateDataPage?.Dispose();
        _ctsDisplayList?.Dispose();
    }

    private void OnDefaultMainMenuClickedMethod()
    {
        _view.DisplayError(ErrorType.Wrong_Input);
        InputService.BlockInputInThreadSleep(1500);
    }

    private void OnDefaultMainDisplayClickedMethod()
    {
        DisableDisplayList();
        _view.DisplayError(ErrorType.Wrong_Input);
    }

    private void OnExitClickedMethod() =>
        Environment.Exit(0);

    private void OnEnterClickedMethod()
    {
        _ctsDisplayList = new();
        _ctsUpdateCountOfPage = new();
        _ctsUpdateDataPage = new();

        _ = UpdatePageAsync(_manualResetEventPrepareData, _ctsUpdateDataPage.Token);
        _ = UpdateCountOfPagesAsync(_manualResetEventPrepareData, _ctsUpdateCountOfPage.Token);
        _ = PrepareDisplayProcessesAsync(_manualResetEventPrepareDisplay, _ctsDisplayList.Token);
    }

    private void OnSearchPageCheckValue(int userIndex)
    {
        if (userIndex < 0 || userIndex > _countOfPages)
        {
            ErrorHelper(ErrorType.Wrong_Input);
            return;
        }

        _currentPage = userIndex;
    }

    private void OnSearchPageReady()
    {
        DisableDisplayList();

        _view.EnterNumberOfPage();
    }

    private void OnFilterProcessesReady()
    {
        DisableDisplayList();

        _view.FilterMemoryOptionsDraw();
    }

    private void ErrorHelper(ErrorType errorType)
    {
        switch (errorType)
        {
            case ErrorType.Wrong_Input: _view.DisplayError(errorType); break;
            case ErrorType.Run_As_Administator: _view.DisplayError(errorType); break;
        }

        InputService.BlockInputInThreadSleep(1500);
        _view.ClearText();
        EnableDisplayList();
    }

    private void HeaderHandler()
    {
        _view.DrawHeader(_currentPage, _countOfPages);
        _view.DrawStats(ProcessService.CalculateTotalMemoryUsage(_processes), _totalMemoryGb, _processes.Length);
    }

    private async Task UpdateProcessesDataAsync()
    {
        AppLogger.Log("UPDATE ASYNC: start method");

        foreach (var process in _processes) // TODO later 0
            process?.Dispose();

        _processes = ProcessService.GetAllProcesses();

        switch (_sortType)
        {
            case SortType.Name:
                ProcessService.SortProcessesByName(_processes);
                break;

            case SortType.Pid:
                ProcessService.SortProcessesByPid(_processes);
                break;

            case SortType.Memory:
                ProcessService.SortProcessesByMemory(_processes);
                break;
        }
    }

    public async Task PrepareDisplayProcessesAsync(ManualResetEvent manualResetEvent, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            manualResetEvent.WaitOne();

            _processesList.Clear();

            _view.ResetColor();
            _view.CursorToTop();

            HeaderHandler();

            for (var i = 0; i < _page?.Length; i++)
                _processesList.Add(new ProcessStruct(_page[i], i, ProcessService.CalculateProcessMemoryUsage(_page[i]), ProcessService.BuildProcessName(_page[i])));

            _view.DrawPage(_processesList);

            await Task.Delay(950, token);
        }
    }

    private async Task UpdateCountOfPagesAsync(ManualResetEvent manualResetEvent, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            manualResetEvent.WaitOne();

            _countOfPages = PageCalculator.CalculateCountOfPages(_processes, CountProcessesInPage);
            await Task.Delay(790, token);
        }
    }

    private async Task UpdatePageAsync(ManualResetEvent manualResetEvent, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UpdateProcessesDataAsync();

            _page = PageCalculator.CalculatePage(_processes, CountProcessesInPage, _currentPage);
            await Task.Delay(810, token);
        }
    }

    private void DisableDisplayList() =>
        _manualResetEventPrepareDisplay.Reset();

    private void EnableDisplayList()
    {
        _manualResetEventPrepareDisplay.Set();
        InputService.BlockInputInThreadSleep(80);
    }
}