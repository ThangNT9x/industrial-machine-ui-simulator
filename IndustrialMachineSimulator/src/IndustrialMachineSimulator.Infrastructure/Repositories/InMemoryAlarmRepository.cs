using IndustrialMachineSimulator.Core.Entities;
using IndustrialMachineSimulator.Core.Interfaces;

namespace IndustrialMachineSimulator.Infrastructure.Repositories;

public class InMemoryAlarmRepository : IAlarmRepository
{
    private readonly List<AlarmRecord> _alarms = new();

    public Task AddAsync(AlarmRecord record)
    {
        _alarms.Add(record);
        return Task.CompletedTask;
    }

    public Task<List<AlarmRecord>> GetAllAsync()
    {
        var result = _alarms
            .OrderByDescending(a => a.Timestamp)
            .ToList();

        return Task.FromResult(result);
    }
}