using IndustrialMachineSimulator.Core.Entities;

namespace IndustrialMachineSimulator.Core.Interfaces;

public interface IAlarmRepository
{
    Task AddAsync(AlarmRecord record);
    Task<List<AlarmRecord>> GetAllAsync();
    Task ClearVisibleAsync();
    Task<List<AlarmRecord>> GetVisibleAsync();
}