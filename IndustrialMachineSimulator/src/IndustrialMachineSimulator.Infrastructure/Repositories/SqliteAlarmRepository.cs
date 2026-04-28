using IndustrialMachineSimulator.Core.Entities;
using IndustrialMachineSimulator.Core.Interfaces;
using IndustrialMachineSimulator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IndustrialMachineSimulator.Infrastructure.Repositories;

public class SqliteAlarmRepository : IAlarmRepository
{
    private readonly DbContextOptions<AppDbContext> _options;

    public SqliteAlarmRepository(DbContextOptions<AppDbContext> options)
    {
        _options = options;
    }

    public async Task AddAsync(AlarmRecord record)
    {
        await using var dbContext = new AppDbContext(_options);

        var entity = new AlarmLogEntity
        {
            Timestamp = record.Timestamp,
            Code = record.Code,
            Severity = record.Severity,
            Message = record.Message
        };

        dbContext.AlarmLogs.Add(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<AlarmRecord>> GetAllAsync()
    {
        await using var dbContext = new AppDbContext(_options);

        return await dbContext.AlarmLogs
            .OrderByDescending(x => x.Timestamp)
            .Select(x => new AlarmRecord
            {
                Id = x.Id,
                Timestamp = x.Timestamp,
                Code = x.Code,
                Severity = x.Severity,
                Message = x.Message
            })
            .ToListAsync();
    }
    public async Task<List<AlarmRecord>> GetVisibleAsync()
    {
        await using var dbContext = new AppDbContext(_options);

        return await dbContext.AlarmLogs
            .Where(x => !x.IsClearedFromUi)
            .OrderByDescending(x => x.Timestamp)
            .Select(x => new AlarmRecord
            {
                Id = x.Id,
                Timestamp = x.Timestamp,
                Code = x.Code,
                Severity = x.Severity,
                Message = x.Message
            })
            .ToListAsync();
    }
    public async Task ClearVisibleAsync()
    {
        await using var dbContext = new AppDbContext(_options);

        var items = await dbContext.AlarmLogs
            .Where(x => !x.IsClearedFromUi)
            .ToListAsync();

        foreach (var item in items)
        {
            item.IsClearedFromUi = true;
            item.ClearedAt = DateTime.Now;
        }

        await dbContext.SaveChangesAsync();
    }
}