using IndustrialMachineSimulator.Core.Entities;

namespace IndustrialMachineSimulator.Core.Interfaces;

public interface IAlarmRepository
{
    Task AddAsync(AlarmRecord alarm, CancellationToken cancellationToken = default);
    Task<List<AlarmRecord>> GetRecentAsync(CancellationToken cancellationToken = default);
}