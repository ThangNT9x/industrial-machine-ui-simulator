using IndustrialMachineSimulator.Core.Entities;
using IndustrialMachineSimulator.Core.Interfaces;
using IndustrialMachineSimulator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IndustrialMachineSimulator.Infrastructure.Repositories;

public class SqliteOperationLogRepository : IOperationLogRepository
{
    private readonly DbContextOptions<AppDbContext> _options;

    public SqliteOperationLogRepository(DbContextOptions<AppDbContext> options)
    {
        _options = options;
    }

    public async Task AddAsync(OperationLogRecord record)
    {
        await using var dbContext = new AppDbContext(_options);

        var entity = new OperationLogEntity
        {
            Timestamp = record.Timestamp,
            Category = record.Category,
            Message = record.Message
        };

        dbContext.OperationLogs.Add(entity);
        await dbContext.SaveChangesAsync();
    }
    public async Task<List<OperationLogRecord>> GetVisibleAsync()
    {
        await using var dbContext = new AppDbContext(_options);

        return await dbContext.OperationLogs
            .Where(x => !x.IsClearedFromUi)
            .OrderByDescending(x => x.Timestamp)
            .Select(x => new OperationLogRecord
            {
                Id = x.Id,
                Timestamp = x.Timestamp,
                Category = x.Category,
                Message = x.Message
            })
            .ToListAsync();
    }
    public async Task ClearVisibleAsync()
    {
        await using var dbContext = new AppDbContext(_options);

        var items = await dbContext.OperationLogs
            .Where(x => !x.IsClearedFromUi)
            .ToListAsync();

        foreach (var item in items)
        {
            item.IsClearedFromUi = true;
            item.ClearedAt = DateTime.Now;
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<List<OperationLogRecord>> GetAllAsync()
    {
        await using var dbContext = new AppDbContext(_options);

        return await dbContext.OperationLogs
            .OrderByDescending(x => x.Timestamp)
            .Select(x => new OperationLogRecord
            {
                Id = x.Id,
                Timestamp = x.Timestamp,
                Category = x.Category,
                Message = x.Message
            })
            .ToListAsync();
    }
}