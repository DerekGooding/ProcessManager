using Process_manager.AppControlleres;
using Process_manager.Module;
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