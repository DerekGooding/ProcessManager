using ProcessManager.Presenters;
using ProcessManager.Models.NativeConsoleMethods;
using ProcessManager.Displays;

namespace Process_manager;
class Program
{
    static void Main()
    {
        var display = new Display();
        AppPresenter appController = new(display);
        NativeConsoleMethod.BlockMouseSelection();
        display.MainMenu();
    }
}