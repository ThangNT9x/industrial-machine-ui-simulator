using IndustrialMachineSimulator.Core.Entities;

namespace IndustrialMachineSimulator.Core.Interfaces;

public interface IAlarmFileLogger
{
    Task WriteAsync(AlarmRecord record);
}