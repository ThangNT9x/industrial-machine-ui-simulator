namespace IndustrialMachineSimulator.Core.Entities;

public class MachineStatusSnapshot
{
    public MachineState State { get; set; } = MachineState.Offline;
    public bool MesConnected { get; set; }
    public bool PlcConnected { get; set; }
    public bool CameraConnected { get; set; }
    public string CurrentRecipe { get; set; } = "None";
    public string CurrentAlarm { get; set; } = string.Empty;
}