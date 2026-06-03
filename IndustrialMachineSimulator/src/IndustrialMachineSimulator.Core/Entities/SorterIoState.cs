namespace IndustrialMachineSimulator.Core.Entities;

public class SorterIoState
{
    public bool IsInConveyorRunning { get; set; }
    public bool IsFeed1Running { get; set; }
    public bool IsFeed2Running { get; set; }
    public bool IsFeed3Running { get; set; }
    public bool IsOutConveyorRunning { get; set; }

    public bool IsInConveyorSensorOn { get; set; }
    public bool IsFeed1SensorOn { get; set; }
    public bool IsFeed2SensorOn { get; set; }
    public bool IsFeed3SensorOn { get; set; }
    public bool IsOutConveyorSensorOn { get; set; }

    public bool HasMaterialAtInConveyor { get; set; }
    public bool HasMaterialAtFeed1 { get; set; }
    public bool HasMaterialAtFeed2 { get; set; }
    public bool HasMaterialAtFeed3 { get; set; }
    public bool HasMaterialAtOutConveyor { get; set; }
}