// TODO: сделать маленький рефактор логики

using ProcessManager.Enums.ErrorTypes;
using ProcessManager.Enums.SortTypes;
using ProcessManager.Interfaces.Iviews;
using ProcessManager.Loggers.AppLoggeres;
using ProcessManager.Models.InputServices;
using ProcessManager.Models.NativeProcessServices;
using ProcessManager.Models.PageCalculators;
using ProcessManager.Models.ProcessServices;
using System.Diagnostics;

namespace ProcessManager.Presenters;

internal class AppPresenter
{
    private const int CountProcessesInPage = 20;

    private readonly float _totalMemoryGb = (float)GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024 * 1024);
    private readonly ManualResetEvent _manualResetEvent = new(true);
    private readonly ConsoleColor _consoleColor = ConsoleColor.White;
    private readonly IView _view;

    private CancellationTokenSource? _ctsDisplayList;
    private CancellationTokenSource? _ctsUpdateDataList;
    private CancellationTokenSource? _ctsUpdateCountOfPage;
    private CancellationTokenSource? _ctsUpdateDataPage;
    private Process[]? _page;

    private SortType _sortType = SortType.None;
    private float _totalMemoryUsage;
    private int _countOfPages;
    private int _currentPage = 0;

    private Process[] _processes = Process.GetProcesses();

    public AppPresenter(IView view)
    {
        _view = view;

        view.OnMenuClicked += OnMenuClickedMethod;
        view.AsyncDisplayPageLoadDataHandler += GetListData;
        view.AsyncDisplayListHeaderHandler += HeaderHandler;
        view.OnSearchPageClicked += OnSearchPageClickedMethod;
        view.OnMainDisplayClicked += OnMainDisplayClickedMethod;
        view.OnManageProcessClicked += OnManageProcessClickedMethod;
        view.OnChangePriorityClicked += OnChangePriorityClickedMethod;
        view.OnFilterProcessesClicked += OnFilterProcessesClickedMethod;
        view.AsyncDisplayProcessCheckDataHandler += CheckProcessNamePointer;
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

            ConsoleKeyInfo consoleKey = InputService.GetHiddenUserInput();

            switch (consoleKey.Key)
            {
                case ConsoleKey.Enter:
                    _ctsDisplayList = new();
                    _ctsUpdateDataList = new();
                    _ctsUpdateCountOfPage = new();
                    _ctsUpdateDataPage = new();

                    _ = UpdatePageAsync(_ctsUpdateDataPage.Token);
                    _ = UpdateCountOfPagesAsync(_ctsUpdateCountOfPage.Token);
                    _ = UpdateProcessesDataAsync(_ctsUpdateDataList.Token);
                    _ = _view.DisplayProcessesAsync(_page, _manualResetEvent, _consoleColor, _ctsDisplayList.Token);

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
            Console.Clear();
            EnableDisplayList();

            ConsoleKeyInfo consoleKey = InputService.GetHiddenUserInput();

            switch (consoleKey.Key)
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

        string userIndexString = Console.ReadLine() ?? String.Empty;

        if (!int.TryParse(userIndexString, out int userIndex))
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

        ConsoleKeyInfo consoleKey = InputService.GetHiddenUserInput();

        switch (consoleKey.Key)
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

        ConsoleKeyInfo consoleKey = InputService.GetHiddenUserInput();

        switch (consoleKey.Key)
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

        string? userIndexString = Console.ReadLine();

        if (!int.TryParse(userIndexString ?? String.Empty, out int userIndex))
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

        ConsoleKeyInfo consoleKey = InputService.GetHiddenUserInput();

        switch (consoleKey.Key)
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
        Console.Clear();
        EnableDisplayList();
    }

    private void HeaderHandler()
    {
        _totalMemoryUsage = 0;
        CalculateMemoryUsage();

        _view.DrawHeader(_currentPage, _countOfPages);
        _view.DrawStats(_totalMemoryUsage, _totalMemoryGb, _processes.Length);
    }

    private void GetListData(Process[] page) =>
        Array.Copy(_page, page, _page.Length);

    private void DisableDisplayList() =>
        _manualResetEvent.Reset();

    private void EnableDisplayList()
    {
        _manualResetEvent.Set();
        InputService.BlockInputInThreadSleep(80);
    }

    private void CalculateMemoryUsage()
    {
        for (int i = 0; i < _processes.Length; i++)
            _totalMemoryUsage += (float)_processes[i].PrivateMemorySize64 / (1024 * 1024);
    }

    //private void CalculateMemoryUsage()
    //{
    //    for (int i = 0; i < _processes.Length; i++)
    //        _totalMemoryUsage += (float)_processes[i].PrivateMemorySize64 / (1024 * 1024);
    //}

    // TODO: Мейби логику из display 1024 + 1024 сюда перенести

    private void CheckProcessNamePointer(Process process, ConsoleColor currentColor, int index)
    {
        if (NativeProcessService.CheckProcessName(process))
            _view.DrawEmptyStroke();
        else
            _view.DrawProcess(process, currentColor, index);
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
            await Task.Delay(770, token);
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