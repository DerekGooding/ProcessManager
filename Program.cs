using ProcessManager.AppPresenters;
using ProcessManager.View;

namespace ProcessManager;

internal static class Program
{
    private static void Main()
    {
        var display = new Display();
        AppPresenter appController = new(display);
        NativeConsoleMethod.BlockMouseSelection();
        display.MainMenu();
    }
}