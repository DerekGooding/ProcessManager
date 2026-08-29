using System.Diagnostics;

namespace ProcessManager.Structs;

public struct ProcessStruct
{
    public Process process;
    public int index;
    public float memoryUsage;
    public string processName;

    public ProcessStruct(Process process, int index, float memoryUsage, string processName)
    {
        this.process = process;
        this.index = index;
        this.memoryUsage = memoryUsage;
        this.processName = processName;
    }
}
