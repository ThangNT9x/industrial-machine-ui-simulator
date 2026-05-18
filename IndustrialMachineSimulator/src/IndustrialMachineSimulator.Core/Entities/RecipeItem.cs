namespace IndustrialMachineSimulator.Core.Entities;

public class RecipeItem
{
    public string RecipeName { get; set; } = string.Empty;
    public string ProductModel { get; set; } = string.Empty;
    public int CycleIntervalMs { get; set; }
    public double CycleOkRate { get; set; }
    public bool EnableNgSimulation { get; set; }
}