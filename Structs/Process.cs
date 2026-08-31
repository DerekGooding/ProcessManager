using System.Diagnostics;

namespace ProcessManager.Structs;

public readonly record struct ProcessStruct(Process Process, int Index, float MemoryUsage, string ProcessName);
