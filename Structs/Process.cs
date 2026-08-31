using System.Diagnostics;

namespace ProcessManager.Structs;

public struct ProcessStruct(Process process, int index, float memoryUsage, string processName)
{
    public Process process = process;
    public int index = index;
    public float memoryUsage = memoryUsage;
    public string processName = processName;
}
