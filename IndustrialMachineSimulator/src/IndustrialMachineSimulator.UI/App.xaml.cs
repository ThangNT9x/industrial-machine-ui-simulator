using IndustrialMachineSimulator.Core.Interfaces;
using IndustrialMachineSimulator.Core.Services;
using IndustrialMachineSimulator.Infrastructure.Configuration;
using IndustrialMachineSimulator.Infrastructure.Data;
using IndustrialMachineSimulator.Infrastructure.Hardware;
using IndustrialMachineSimulator.Infrastructure.Logging;
using IndustrialMachineSimulator.Infrastructure.Networking;
using IndustrialMachineSimulator.Infrastructure.Repositories;
using IndustrialMachineSimulator.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;

namespace IndustrialMachineSimulator.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Directory.CreateDirectory("Data");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=Data/app.db")
            .Options;

        using (var dbContext = new AppDbContext(options))
        {
            dbContext.Database.EnsureCreated();
            dbContext.Database.ExecuteSqlRaw("""
                                            CREATE TABLE IF NOT EXISTS OperationLogs (
                                                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                                Timestamp TEXT NOT NULL,
                                                Category TEXT NOT NULL,
                                                Message TEXT NOT NULL
                                            );
                                            """);
        }

        var services = new ServiceCollection();

        services.AddSingleton(options);

        services.AddSingleton<ICameraService, MockCameraService>();
        services.AddSingleton<IPlcService, MockPlcService>();
        services.AddSingleton<IMesClient, MockMesClient>();
        services.AddSingleton<IAlarmRepository, SqliteAlarmRepository>();
        services.AddSingleton<IOperationLogRepository, SqliteOperationLogRepository>();
        services.AddSingleton<IAlarmFileLogger, AlarmFileLogger>();
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<IOperationFileLogger, OperationFileLogger>();
        services.AddSingleton<IConfigService, JsonConfigService>();
        services.AddSingleton<MachineController>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}