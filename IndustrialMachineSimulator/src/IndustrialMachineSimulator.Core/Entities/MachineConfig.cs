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
    public string CurrentRecipeName { get; set; } = "Default Recipe";
    public string CurrentProductModel { get; set; } = "MODEL-001";
    public bool EnableNgSimulation { get; set; } = true;
    public List<RecipeItem> Recipes { get; set; } = new();

}