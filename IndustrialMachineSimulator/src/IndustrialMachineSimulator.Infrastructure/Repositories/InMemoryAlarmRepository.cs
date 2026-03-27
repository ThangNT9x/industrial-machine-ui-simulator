using IndustrialMachineSimulator.Core.Entities;
using IndustrialMachineSimulator.Core.Interfaces;

namespace IndustrialMachineSimulator.Infrastructure.Repositories;

public class InMemoryAlarmRepository : IAlarmRepository
{
    private readonly List<AlarmRecord> _alarms = new();

    public Task AddAsync(AlarmRecord alarm, CancellationToken cancellationToken = default)
    {
        _alarms.Add(alarm);
        return Task.CompletedTask;
    }

    public Task<List<AlarmRecord>> GetRecentAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_alarms.OrderByDescending(a => a.Timestamp).Take(20).ToList());
    }
}