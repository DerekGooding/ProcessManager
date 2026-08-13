using ErrorTypes;
using ProcessManager.Display.Engine;
using SortTypes;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessManager.Display;

internal class Display
{
    [DllImport("kernel32.dll")]
    public static extern void QueryFullProcessImageNameW();

    Process[]? page;
    Process[] processes = DisplayEngine.ProcessesListLoad();
    SortType currentSortType = SortType.None;

    private const string LOGO = @"
                                ..:                     
                                .-:                     
               -====             .*=         =--=       
               :--+=-            =#+        ====        
                 -:: -           :%= ++    +*+##*       
                   :+           :**-+-   ++#%=*++       
                     :#+         --=-  -*+=++           
                       #+        :--- : ===+=*          
                       - **     ==+= =  ==:=            
                 - --  :===#-+++*%#:+**#%%#::---++:- .::
             :-         -*+-*-=%%@%*%%%%+#+++ =--==:    
                            :*##%#%%%++   +=--+=*+=---=-
                               **#.-#=-==.=+*#+==       
                               =+= .:+*#%#+:.:          
                    ##=-+#*    -= . .--.#-#.: .-        
                *+#**%@#*      :=*= .***=+*#%*+ .       
              *#=*-=#++       =-=.=  +##%%++#%%%%#.++   
             #*#%%#@#*         --.   +##%%%#+%**@@#*+#+ 
            ###%%%%#           ::.   %#%%%#  ##%%%##*  
            ##%%%              :-:.  %#%%      ##      
                               =-:. #%%%              
                               =-:  *%%              
                                :
";

    private string[] menuOptions =
        {
            "Enter: Processes Menu",
            "Esc: Exit",
        };

    private string[] filterOptions =
       {
            "1. Filter by Name",
            "2. Filter by PID",
            "3. Filter by Memory",
       };

    private string[] processOptions =
       {
            "1. Kill Process",
            "2. Close main process window",
            "3. Open process file directory",
            "4. Change priority of process"
       };

    private string[] changePriorityOptions =
       {
            "1. RealTime ( MAX )",
            "2. High",
            "3. AboveNormal",
            "4. Normal",
            "5. BelowNormal",
            "6. Idle ( AFK )",
       };

    public void MainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(LOGO);
            Console.ResetColor();

            for (int i = 0; i < menuOptions.Length; i++)
            {
                int leftPartLength = 0;
                Console.SetCursorPosition(75, 12 + i);

                for (int j = 0; j < menuOptions[i].Length; j++)
                {
                    if (menuOptions[i][j] == ':' && menuOptions[i][j + 1] == ' ') leftPartLength = j;
                }

                for (int l = 0; l < leftPartLength; l++)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(menuOptions[i][l]);
                }

                Console.ResetColor();

                for (int k = leftPartLength; k < menuOptions[i].Length; k++)
                {
                    Console.Write(menuOptions[i][k]);
                }

                Console.WriteLine();
            }

            ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
            switch (consoleKey.Key)
            {
                case ConsoleKey.Enter: ProcessesListDisplay(); break;
                case ConsoleKey.Backspace:
                case ConsoleKey.Escape: DisplayEngine.Exit(); break;
                default: DisplayError(ErrorType.Wrong_Input); continue;
            }
        }
    }

    private async Task ProcessesListDisplay()
    {
        var cts = new CancellationTokenSource();
        ConsoleKeyInfo consoleKey = default;
        const int COUNT_PROCESSES_IN_PAGE = 20;
        int countOfPages = processes.Length / COUNT_PROCESSES_IN_PAGE;
        int currentPage = 0;

        if (processes.Length % 10 != 0)
            countOfPages++;

        var taskLisdLoad = UpdateProcesses();
        var taskDisplayList = ProcessesListAsync(cts);

        while (true)
        {
            consoleKey = DisplayEngine.GetUserInput();

            switch (consoleKey.Key)
            {
                case ConsoleKey.E:
                    if (currentPage < countOfPages - 1) currentPage++;
                    break;

                case ConsoleKey.Q:
                    if (currentPage > 0) currentPage--;
                    break;

                case ConsoleKey.Oem3:
                    {
                        if (cts != null) cts.Cancel();
                        if (!ProcessesManage()) continue;
                        cts = new CancellationTokenSource();
                        taskDisplayList = ProcessesListAsync(cts);
                    }
                    break;

                case ConsoleKey.Tab:
                    {
                        {
                            if (cts != null) cts.Cancel();
                            if (!ProcessesFilter()) continue;
                            cts = new CancellationTokenSource();
                            taskDisplayList = ProcessesListAsync(cts);
                        }
                    }
                    break;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    {
                        if (cts != null) cts.Cancel();
                    }
                    return;
            }
        }

        async Task ProcessesListAsync(CancellationTokenSource tokenSource)
        {
            while (!tokenSource.Token.IsCancellationRequested)
            {
                var currentProcesses = processes;

                if (currentProcesses == null || currentProcesses.Length == 0)
                {
                    await Task.Delay(100);
                    continue;
                }

                page = currentProcesses
                   .Skip(COUNT_PROCESSES_IN_PAGE * currentPage)
                   .Take(COUNT_PROCESSES_IN_PAGE)
                   .ToArray();


                double totalMemoryUsage = 0;

                Console.Clear();
                Console.WriteLine("'Q' left | 'E' right | 'TAB' filter | '`' manage |'ESC / BACKSPACE' exit", Console.ForegroundColor = ConsoleColor.Gray);
                Console.WriteLine($"Current page: {currentPage + 1}|{countOfPages}\n\n");

                for (int i = 0; i < currentProcesses.Length; i++)
                {
                    totalMemoryUsage += currentProcesses[i].PrivateMemorySize64 / (1024 * 1024);
                    if (i == currentProcesses.Length - 1)
                    {
                        Console.WriteLine($"Total memory usage: {totalMemoryUsage} | Count of processes: {currentProcesses.Length}");
                    }
                }

                for (int i = 0; i < page.Length; i++)
                {
                    ConsoleColor currentColor;

                    if (i % 2 == 0) currentColor = ConsoleColor.DarkGray;
                    else currentColor = ConsoleColor.Gray;

                    double memoryUsage = page[i].PrivateMemorySize64 / (1024 * 1024); // convert byte to MB
                    string processNameModifier = page[i].ProcessName;
                    if (page[i].ProcessName.Length >= 25) processNameModifier = page[i].ProcessName[..25] + "...";

                    Console.Write($"| CID: {currentProcesses.IndexOf(page[i]),-2} \t", Console.ForegroundColor = currentColor);
                    Console.Write($"| Name: {processNameModifier,-25} \t", Console.ForegroundColor = ConsoleColor.Yellow); // TODO сделать так чтобы расширение файла выводило.
                    Console.Write($"| PID: {page[i].Id,-5} \t", Console.ForegroundColor = currentColor);
                    Console.Write($"| Memory: {memoryUsage} MB\n", Console.ForegroundColor = ConsoleColor.Green);
                    Console.ResetColor();
                }

                await Task.Delay(950);
            }
        }

        bool ProcessesFilter()
        {
            Console.WriteLine();
            for (int i = 0; i < filterOptions.Length; i++)
                Console.WriteLine(filterOptions[i]);

            ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
            switch (consoleKey.Key)
            {
                case ConsoleKey.D1:
                    currentSortType = SortType.Name;
                    DisplayEngine.SortByName(processes);
                    return true;

                case ConsoleKey.D2:
                    currentSortType = SortType.PID;
                    DisplayEngine.SortByPID(processes);
                    return true;

                case ConsoleKey.D3:
                    currentSortType = SortType.Memory;
                    DisplayEngine.SortByMemory(processes);
                    return true;

                case ConsoleKey.Backspace:
                case ConsoleKey.Escape:
                    {
                        cts = new CancellationTokenSource();
                        taskDisplayList = ProcessesListAsync(cts);
                    }
                    return false;
            }

            return false;
        }

        bool ProcessesManage()
        {
            while (true)
            {
                Console.Write("Enter a CID: ");
                string? userIndexString = Console.ReadLine();

                if (!DisplayEngine.NumberInit(userIndexString ?? String.Empty, out int userIndex))
                {
                    DisplayError(ErrorType.Wrong_Input);
                    continue;
                }

                Console.WriteLine();

                for (int i = 0; i < processOptions.Length; i++)
                    Console.WriteLine(processOptions[i]);

                Console.WriteLine("Choose option");

                ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
                switch (consoleKey.Key)
                {
                    case ConsoleKey.D1:
                        {
                            if (!DisplayEngine.KillProcess(processes, userIndex))
                                DisplayError(ErrorType.Run_As_Administator);
                        }
                        return true;

                    case ConsoleKey.D2:
                        {
                            if (!DisplayEngine.CloseMainWindowProcess(processes, userIndex))
                                DisplayError(ErrorType.Run_As_Administator);
                        }
                        return true;

                    case ConsoleKey.D3:
                        {
                            if (!DisplayEngine.OpenFileDirectoryProcess(processes, userIndex))
                                DisplayError(ErrorType.Run_As_Administator);
                        }
                        return true;

                    case ConsoleKey.D4:
                        {
                            if (!ChangePriority(userIndex))
                                return false;
                        } 
                        return true;  // TODO: ПРОРВЕРИТЬ ТУТ ЛОГИКУ

                    case ConsoleKey.Backspace:
                    case ConsoleKey.Escape:
                        {
                            cts = new CancellationTokenSource();
                            taskDisplayList = ProcessesListAsync(cts);
                        }
                        return false;
                }

                return false;
            }

            bool ChangePriority(int userIndex)
            {
                while (true)
                {
                    Console.WriteLine();
                    for (int i = 0; i < changePriorityOptions.Length; i++)
                        Console.WriteLine(changePriorityOptions[i]);

                    ConsoleKeyInfo consoleKey = DisplayEngine.GetUserInput();
                    switch (consoleKey.Key)
                    {
                        case ConsoleKey.D1:
                            DisplayEngine.СhangeProcessPriority(processes, userIndex, ProcessPriorityClass.RealTime);
                            return true;

                        case ConsoleKey.D2:
                            DisplayEngine.СhangeProcessPriority(processes, userIndex, ProcessPriorityClass.High);
                            return true;

                        case ConsoleKey.D3:
                            DisplayEngine.СhangeProcessPriority(processes, userIndex, ProcessPriorityClass.AboveNormal);
                            return true;

                        case ConsoleKey.D4:
                            DisplayEngine.СhangeProcessPriority(processes, userIndex, ProcessPriorityClass.Normal);
                            return true;

                        case ConsoleKey.D5:
                            DisplayEngine.СhangeProcessPriority(processes, userIndex, ProcessPriorityClass.BelowNormal);
                            return true;

                        case ConsoleKey.D6:
                            DisplayEngine.СhangeProcessPriority(processes, userIndex, ProcessPriorityClass.Idle);
                            return true;

                        case ConsoleKey.Backspace:
                        case ConsoleKey.Escape: return false;
                    }

                    return false;
                }
            }
        }
    }

    private async Task UpdateProcesses()
    {
        while (true)
        {
            processes = DisplayEngine.ProcessesListLoad();
            switch (currentSortType)
            {
                case SortType.Name: DisplayEngine.SortByName(processes); break;
                case SortType.PID: DisplayEngine.SortByPID(processes); break;
                case SortType.Memory: DisplayEngine.SortByMemory(processes); break;
            }

            await Task.Delay(900);
        }
    }

    private bool DisplayError(ErrorType errorType)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;

        switch (errorType)
        {
            case ErrorType.Run_As_Administator: Console.WriteLine("Error #1: Try to run the program as administrator"); break;
            case ErrorType.Wrong_Input: Console.WriteLine("Error #2: Wrong input, make sure you have entered it correctly."); break;
            default: Console.WriteLine("Error 555: Unknown error"); break;
        }

        Console.WriteLine();
        Thread.Sleep(1500);
        Console.ResetColor();

        return true;
    }
}