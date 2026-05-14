namespace IndustrialMachineSimulator.Infrastructure.Data;

public class OperationLogEntity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsClearedFromUi { get; set; } = false;
    public DateTime? ClearedAt { get; set; }
}