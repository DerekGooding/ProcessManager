using ProcessManager.AppPresenters;
using ProcessManager.View;
using ProcessManager.Models;

namespace ProcessManager;

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