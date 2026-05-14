namespace IndustrialMachineSimulator.Core.Entities;

public class OperationLogRecord
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}