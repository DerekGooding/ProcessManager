//TODO: Convert console read line read key to the DLL import VK_SPACE = 0x20 or sohmething like that; ( medium )

using ProcessManager.Enums.ErrorTypes;
using ProcessManager.Enums.SortTypes;
using ProcessManager.Interfaces.Iviews;
using ProcessManager.Loggers.AppLoggeres;
using ProcessManager.Models.InputServices;
using ProcessManager.Models.PageCalculators;
using ProcessManager.Models.ProcessServices;
using ProcessManager.Structs;
using System.Diagnostics;

namespace ProcessManager.Presenters;

internal class AppPresenter
{
    private const int CountProcessesInPage = 20;

    private readonly ManualResetEvent _manualResetEvent = new(true);
    private readonly Lock _locker = new();
    private readonly IView _view;
    private readonly float _totalMemoryGb = (float)GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024 * 1024);

    private CancellationTokenSource? _ctsDisplayList;
    private CancellationTokenSource? _ctsUpdateDataList;
    private CancellationTokenSource? _ctsUpdateCountOfPage;
    private CancellationTokenSource? _ctsUpdateDataPage;
    private Process[]? _page;
    private List<ProcessStruct> _processesList = new();

    private SortType _sortType = SortType.None;
    private int _countOfPages;
    private int _currentPage = 0;

    private Process[] _processes = Process.GetProcesses();

    public AppPresenter(IView view)
    {
        _view = view;

        view.OnMenuClicked += OnMenuClickedMethod;
        view.OnSearchPageClicked += OnSearchPageClickedMethod;
        view.OnMainDisplayClicked += OnMainDisplayClickedMethod;
        view.OnManageProcessClicked += OnManageProcessClickedMethod;
        view.OnChangePriorityClicked += OnChangePriorityClickedMethod;
        view.OnFilterProcessesClicked += OnFilterProcessesClickedMethod;
    }

    private void OnMenuClickedMethod()
    {
        _ctsUpdateCountOfPage?.Dispose();
        _ctsUpdateDataPage?.Dispose();
        _ctsUpdateDataList?.Dispose();
        _ctsDisplayList?.Dispose();

        while (true)
        {
            _view.MainMenuDraw();

            switch (InputService.GetHiddenUserInput().Key)
            {
                case ConsoleKey.Enter:
                    _ctsDisplayList = new();
                    _ctsUpdateDataList = new();
                    _ctsUpdateCountOfPage = new();
                    _ctsUpdateDataPage = new();

                    _ = UpdatePageAsync(_ctsUpdateDataPage.Token);
                    _ = UpdateCountOfPagesAsync(_ctsUpdateCountOfPage.Token);
                    _ = UpdateProcessesDataAsync(_ctsUpdateDataList.Token);
                    _ = PrepareDisplayProcessesAsync(_manualResetEvent, _ctsDisplayList.Token);

                    _view.MainDisplay();
                    break;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;

                default:
                    _view.DisplayError(ErrorType.Wrong_Input);
                    InputService.BlockInputInThreadSleep(1500);
                    continue;
            }
        }
    }

    private void OnMainDisplayClickedMethod()
    {
        while (true)
        {
            _view.ClearText();
            EnableDisplayList();

            switch (InputService.GetHiddenUserInput().Key)
            {
                case ConsoleKey.E:
                    if (_currentPage < _countOfPages) _currentPage++;
                    continue;

                case ConsoleKey.Q:
                    if (_currentPage > 0) _currentPage--;
                    continue;

                case ConsoleKey.Oem3:
                    OnManageProcessClickedMethod();
                    break;

                case ConsoleKey.Tab:
                    OnFilterProcessesClickedMethod();
                    break;

                case ConsoleKey.F1:
                    OnSearchPageClickedMethod();
                    break;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    _ctsUpdateCountOfPage?.Cancel();
                    _ctsUpdateDataPage?.Cancel();
                    _ctsUpdateDataList?.Cancel();
                    _ctsDisplayList?.Cancel();
                    return;

                default:
                    DisableDisplayList();
                    _view.DisplayError(ErrorType.Wrong_Input);
                    continue;
            }
        }
    }

    private void OnManageProcessClickedMethod()
    {
        DisableDisplayList();

        _view.EnterCid();

        if (!int.TryParse(InputService.GetUserMultiInput(), out int userIndex))
        {
            ErrorHelper(ErrorType.Wrong_Input);
            return;
        }

        if (userIndex < 0 || userIndex > _processes.Length - 1)
        {
            ErrorHelper(ErrorType.Wrong_Input);
            return;
        }

        _view.ManageOptionDraw();

        switch (InputService.GetHiddenUserInput().Key)
        {
            case ConsoleKey.D1:
                if (!ProcessService.KillProcess(_processes, userIndex))
                    ErrorHelper(ErrorType.Run_As_Administator);
                return;

            case ConsoleKey.D2:
                if (!ProcessService.CloseMainWindowProcess(_processes, userIndex))
                    ErrorHelper(ErrorType.Run_As_Administator);
                return;

            case ConsoleKey.D3:
                if (!ProcessService.OpenFileDirectoryProcess(_processes, userIndex))
                    ErrorHelper(ErrorType.Run_As_Administator);
                return;

            case ConsoleKey.D4:
                OnChangePriorityClickedMethod(userIndex);
                return;

            case ConsoleKey.Backspace:
            case ConsoleKey.Escape:
                return;

            default:
                ErrorHelper(ErrorType.Wrong_Input);
                return;
        }

    }

    private void OnChangePriorityClickedMethod(int userIndex)
    {
        _view.ChangePriorityOptionDraw();

        switch (InputService.GetHiddenUserInput().Key)
        {
            case ConsoleKey.D1:
                ProcessService.ChangePriorityProcess(_processes, userIndex, ProcessPriorityClass.RealTime);
                return;

            case ConsoleKey.D2:
                ProcessService.ChangePriorityProcess(_processes, userIndex, ProcessPriorityClass.High);
                return;

            case ConsoleKey.D3:
                ProcessService.ChangePriorityProcess(_processes, userIndex, ProcessPriorityClass.AboveNormal);
                return;

            case ConsoleKey.D4:
                ProcessService.ChangePriorityProcess(_processes, userIndex, ProcessPriorityClass.Normal);
                return;

            case ConsoleKey.D5:
                ProcessService.ChangePriorityProcess(_processes, userIndex, ProcessPriorityClass.BelowNormal);
                return;

            case ConsoleKey.D6:
                ProcessService.ChangePriorityProcess(_processes, userIndex, ProcessPriorityClass.Idle);
                return;

            case ConsoleKey.Backspace:
            case ConsoleKey.Escape:
                return;

            default:
                ErrorHelper(ErrorType.Wrong_Input);
                break;
        }
    }

    private void OnSearchPageClickedMethod()
    {
        DisableDisplayList();

        _view.EnterNumberOfPage();

        if (!int.TryParse(InputService.GetUserMultiInput(), out int userIndex))
        {
            ErrorHelper(ErrorType.Wrong_Input);
            return;
        }

        if (userIndex < 0 || userIndex > _countOfPages)
        {
            ErrorHelper(ErrorType.Wrong_Input);
            return;
        }

        _currentPage = userIndex;
    }

    private void OnFilterProcessesClickedMethod()
    {
        DisableDisplayList();

        _view.FilterMemoryOptionsDraw();

        switch (InputService.GetHiddenUserInput().Key)
        {
            case ConsoleKey.D1:
                _sortType = SortType.Name;
                ProcessService.SortProcessesByName(ref _processes);
                return;

            case ConsoleKey.D2:
                _sortType = SortType.Pid;
                ProcessService.SortProcessesByPid(ref _processes);
                return;

            case ConsoleKey.D3:
                _sortType = SortType.Memory;
                ProcessService.SortProcessesByMemory(ref _processes);
                return;

            case ConsoleKey.Backspace:
            case ConsoleKey.Escape:
                return;

            default:
                ErrorHelper(ErrorType.Wrong_Input);
                return;

        }
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

    private void DisableDisplayList() =>
        _manualResetEvent.Reset();

    private void EnableDisplayList()
    {
        _manualResetEvent.Set();
        InputService.BlockInputInThreadSleep(80);
    }

    private async Task UpdateProcessesDataAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            AppLogger.Log("UPDATE ASYNC: start method");

            _processes = Process.GetProcesses();

            switch (_sortType)
            {
                case SortType.Name:
                    ProcessService.SortProcessesByName(ref _processes);
                    break;

                case SortType.Pid:
                    ProcessService.SortProcessesByPid(ref _processes);
                    break;

                case SortType.Memory:
                    ProcessService.SortProcessesByMemory(ref _processes);
                    break;
            }

            // TODO: Dispose old array to clean RAM 

            await Task.Delay(770, token);
        }
    }

    public async Task PrepareDisplayProcessesAsync(ManualResetEvent manualResetEvent, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            manualResetEvent.WaitOne();

            lock (_locker)
            {
                _processesList.Clear();

                _view.ResetColor();
                _view.CursorToTop();

                HeaderHandler();

                for (int i = 0; i < _page.Length; i++) // TODO: MARKER
                    _processesList.Add(new ProcessStruct(_page[i], i, ProcessService.CalculateProcessMemoryUsage(_page[i]), ProcessService.BuildProcessName(_page[i])));

                _view.DrawPage(_processesList);
            }
            await Task.Delay(950, token);
        }
    }

    private async Task UpdateCountOfPagesAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            _countOfPages = PageCalculator.CalculateCountOfPages(_processes, CountProcessesInPage);
            await Task.Delay(790, token);
        }
    }

    private async Task UpdatePageAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            _page = PageCalculator.CalculatePage(_processes, CountProcessesInPage, _currentPage);
            await Task.Delay(810, token);
        }
    }
}