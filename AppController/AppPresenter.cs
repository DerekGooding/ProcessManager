// TODO: Как в main on clicked может вызывать метод а може тпросто while сюда завести что более вероятно логично
// TODO: ПОправить namespace'ы

using Process_manager.Engine;
using Process_manager.Interfaces;
using Process_manager.Model;
using Process_manager.Module;
using ProcessManager.AppLoggeres;
using ProcessManager.ErrorTypes;
using ProcessManager.SortTypes;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Process_manager.AppControlleres;

internal class AppPresenter
{
    private const int CountProcessesInPage = 20;

    private readonly float _totalMemoryGb = (float)GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024 * 1024);

    private CancellationTokenSource? _ctsDisplayList;
    private CancellationTokenSource? _ctsUpdateDataList;
    private CancellationTokenSource? _ctsUpdateCountOfPage;
    private CancellationTokenSource? _ctsUpdateDataPage;

    private ConsoleColor _consoleColor = ConsoleColor.White;
    private SortType _sortType = SortType.None;
    private IView _view;

    private float _totalMemoryUsage;
    private int _countOfPages;
    private int _currentPage = 0;

    private ManualResetEvent _manualResetEvent = new ManualResetEvent(true);
    private Process[] _processes = Process.GetProcesses();
    private Process[] _page;

    public AppPresenter(IView view)
    {
        _view = view;

        view.OnMenuClicked += OnMenuClickedMethod;
        view.OnMainDisplayClicked += OnMainDisplayClickedMethod;
        view.OnManageProcessClicked += OnManageProcessClickedMethod;
        view.OnChangePriorityClicked += OnChangePriorityClickedMethod;
        view.OnSearchPageClicked += OnSearchPageClickedMethod;
        view.OnFilterProcessesClicked += OnFilterProcessesClickedMethod;

        view.AsyncDisplayListHeaderHandler += HeaderHandler;
        view.AsyncDisplayPageLoadDataHandler += GetListData;
        view.AsyncDisplayProcessCheckDataHandler += CheckMemoryPointer;
    }

    private void OnMenuClickedMethod()
    {
        _ctsUpdateCountOfPage?.Dispose();
        _ctsUpdateDataPage?.Dispose();
        _ctsUpdateDataList?.Dispose();
        _ctsDisplayList?.Dispose();

        ConsoleKeyInfo consoleKey = InputService.GetHiddenUserInput();

        switch (consoleKey.Key)
        {
            case ConsoleKey.Enter:
                AppLogger.Log("User choice: 'enter'");

                _ctsUpdateDataList = new();
                _ctsDisplayList = new();
                _ctsUpdateCountOfPage = new();
                _ctsUpdateDataPage = new();

                _ = UpdateCountOfPagesAsync(_ctsUpdateCountOfPage.Token);
                _ = UpdatePageAsync(_ctsUpdateDataPage.Token);
                _ = UpdateProcessesDataAsync(_ctsUpdateDataList.Token);
                _ = _view.DisplayProcessesAsync(_ctsDisplayList.Token, _page, _manualResetEvent, _consoleColor);

                _view.MainDisplay();
                break;

            case ConsoleKey.Backspace:
            case ConsoleKey.Escape:
                AppLogger.Log("User choice: 'exit'");

                Environment.Exit(0);
                break;

            default:
                _view.DisplayError(ErrorType.Wrong_Input);
                InputService.BlockInputInThreadSleep(1500);
                _view.MainMenu();
                break;

        }
    }

    private void OnMainDisplayClickedMethod()
    {
        Console.Clear();
        EnableDisplayList();

        while (true)
        {
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
                    _view.ManageProcess();
                    break;

                case ConsoleKey.Tab:
                    _view.FilterProcesses();
                    break;

                case ConsoleKey.F1:
                    _view.SearchPage();
                    break;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    _ctsUpdateCountOfPage?.Cancel();
                    _ctsUpdateDataPage?.Cancel();
                    _ctsUpdateDataList?.Cancel();
                    _ctsDisplayList?.Cancel();

                    _view.MainDisplay();
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
            ErrorHelper(ErrorType.Wrong_Input);

        if (userIndex < 0 || userIndex > _processes.Length - 1)
            ErrorHelper(ErrorType.Wrong_Input);

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
                _view.ChangePriority(userIndex);
                return;

            case ConsoleKey.Backspace:
            case ConsoleKey.Escape:
                return;

            default:
                ErrorHelper(ErrorType.Wrong_Input);
                break;
        }
    }

    private void OnChangePriorityClickedMethod(int userIndex)
    {
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
            ErrorHelper(ErrorType.Wrong_Input);


        if (userIndex < 0 || userIndex > _countOfPages)
            ErrorHelper(ErrorType.Wrong_Input);


        _currentPage = userIndex;
    }

    private void OnFilterProcessesClickedMethod()
    {
        DisableDisplayList();

        ConsoleKeyInfo consoleKey = InputService.GetHiddenUserInput();
        switch (consoleKey.Key)
        {
            case ConsoleKey.D1:
                _sortType = SortType.Name;
                ProcessService.SortProcessesByName(_processes);
                return;

            case ConsoleKey.D2:
                _sortType = SortType.Pid;
                ProcessService.SortProcessesByPid(_processes);
                return;

            case ConsoleKey.D3:
                _sortType = SortType.Memory;
                ProcessService.SortProcessesByMemory(_processes);
                return;

            case ConsoleKey.Backspace:
            case ConsoleKey.Escape:
                return;

            default:
                ErrorHelper(ErrorType.Wrong_Input);
                break;
        }
    }

    private void ErrorHelper(ErrorType errorType)
    {
        switch (errorType)
        {
            case ErrorType.Wrong_Input: _view.DisplayError(ErrorType.Wrong_Input); break;
            case ErrorType.Run_As_Administator: _view.DisplayError(ErrorType.Wrong_Input); break;
        }

        InputService.BlockInputInThreadSleep(1500);
        EnableDisplayList();
    }

    private void HeaderHandler()
    {
        _totalMemoryUsage = 0;
        CalculateMemoryUsage();

        _view.DrawHeader(_currentPage, _countOfPages);
        _view.DrawStats(_totalMemoryUsage, _totalMemoryGb, _processes.Length);
    }

    private void GetListData(Process[] page)
    {
        Array.Copy(_page, page, _page.Length);
    }

    private void EnableDisplayList([CallerMemberName] string callerName = "")
    {
        AppLogger.Log($"[{callerName}] Enable display async");
        _manualResetEvent.Set();
        InputService.BlockInputInThreadSleep(1060);
    }

    private void DisableDisplayList([CallerMemberName] string callerName = "")
    {
        AppLogger.Log($"[{callerName}] Disable display async");
        _manualResetEvent.Reset();
    }

    private void CalculateMemoryUsage()
    {
        for (int i = 0; i < _processes.Length; i++)
            _totalMemoryUsage += (float)_processes[i].PrivateMemorySize64 / (1024 * 1024);
    }

    private void CheckMemoryPointer(Process process, ConsoleColor currentColor, int index)
    {
        if (NativeProcessService.CheckProcessMemoryPointer(process))
        {
            _view.DrawEmptyStroke();
        }
        else
        {
            _view.DrawProcess(process, currentColor, index);
        }
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
                    ProcessService.SortProcessesByName(_processes);
                    break;

                case SortType.Pid:
                    ProcessService.SortProcessesByPid(_processes);
                    break;

                case SortType.Memory:
                    ProcessService.SortProcessesByMemory(_processes);
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