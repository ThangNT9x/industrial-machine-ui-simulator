using IndustrialMachineSimulator.Core.Entities;

namespace IndustrialMachineSimulator.Core.Interfaces;

public interface IOperationLogRepository
{
    Task AddAsync(OperationLogRecord record);
    Task<List<OperationLogRecord>> GetAllAsync();
    Task<List<OperationLogRecord>> GetVisibleAsync();
    Task ClearVisibleAsync();
}