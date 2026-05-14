using IndustrialMachineSimulator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IndustrialMachineSimulator.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<AlarmLogEntity> AlarmLogs => Set<AlarmLogEntity>();

    public DbSet<OperationLogEntity> OperationLogs => Set<OperationLogEntity>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}