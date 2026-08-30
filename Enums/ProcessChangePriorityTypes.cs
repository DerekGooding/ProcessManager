namespace ProcessManager.Enums.ProcessChangePriorityTypes;

public enum ProcessChangePriorityType : short
{
    None = 0,
    RealTime,
    High,
    AboveNormal,
    Normal,
    BelowNormal,
    Idle,
}
