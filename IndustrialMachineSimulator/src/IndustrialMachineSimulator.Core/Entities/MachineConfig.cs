namespace IndustrialMachineSimulator.Core.Entities;

public class MachineConfig
{
    public string AppTitle { get; set; } = "SM_S928_ROUTER_LASER_SIMULATOR";
    public string OsVersion { get; set; } = "0.0.1";
    public string LaserTimeText { get; set; } = "2000 / 20000H (100 %)";
    public string EngineerPassword { get; set; } = "engineer123";
    public string DeveloperPassword { get; set; } = "developer123";
    public int CycleIntervalMs { get; set; } = 3000;
    public double CycleOkRate { get; set; } = 0.8;

}