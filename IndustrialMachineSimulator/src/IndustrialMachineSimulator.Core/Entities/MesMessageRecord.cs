namespace IndustrialMachineSimulator.Core.Entities;

public class MesMessageRecord
{
    public DateTime Timestamp { get; set; }
    public string Direction { get; set; } = string.Empty;   // TX / RX / SYS
    public string MessageType { get; set; } = string.Empty; // Connect / StartJob / CycleResult / Alarm ...
    public string Payload { get; set; } = string.Empty;
}