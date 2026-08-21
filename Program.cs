using ProcessManager.Displays;
using ProcessManager.Displays.Engine.NativeMethodes;

namespace Process_manager;

class Program
{
    static void Main()
    {
        NativeMethod.BlockMouseSelection();
        var display = new Display();
        display.MainMenu();
    }
}