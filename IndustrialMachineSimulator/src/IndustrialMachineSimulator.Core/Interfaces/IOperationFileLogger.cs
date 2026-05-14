using IndustrialMachineSimulator.Core.Entities;

namespace IndustrialMachineSimulator.Core.Interfaces;

public interface IOperationFileLogger
{
    Task WriteAsync(OperationLogRecord record);
}