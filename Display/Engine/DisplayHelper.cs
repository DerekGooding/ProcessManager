using ProcessManager.AppLoggeres;
using ProcessManager.Displays.Engine.ConsoleHelpers;
using System.ComponentModel;
using System.Diagnostics;

namespace ProcessManager.Displays.Engine.DisplayHelpers;

internal class DisplayHelper
{
    public static void ExitProgram() =>
        Environment.Exit(0);

    public static bool IsNumber(string stringEnter, out int value)
    {
        AppLogger.Log("Init user number");
        if (!int.TryParse(stringEnter, out value)) return false;
        else return true;
    }

    public static void EnableDisplayList(ref bool isListDisplayed)
    {
        isListDisplayed = true;
        ConsoleHelper.BlockInputInThreadSleep(1060);
    }

    public static void DisableDisplayList(ref bool isListDisplayed)
    {
        isListDisplayed = false;
    }
}