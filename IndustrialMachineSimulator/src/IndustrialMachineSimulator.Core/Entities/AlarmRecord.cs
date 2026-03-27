namespace IndustrialMachineSimulator.Core.Entities;

public class AlarmRecord
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Warning";
}