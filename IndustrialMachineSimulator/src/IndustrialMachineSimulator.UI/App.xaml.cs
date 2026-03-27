using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using IndustrialMachineSimulator.Core.Interfaces;
using IndustrialMachineSimulator.Core.Services;
using IndustrialMachineSimulator.Infrastructure.Hardware;
using IndustrialMachineSimulator.Infrastructure.Logging;
using IndustrialMachineSimulator.Infrastructure.Networking;
using IndustrialMachineSimulator.Infrastructure.Repositories;
using IndustrialMachineSimulator.UI.ViewModels;

namespace IndustrialMachineSimulator.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddSingleton<ICameraService, MockCameraService>();
        services.AddSingleton<IPlcService, MockPlcService>();
        services.AddSingleton<IMesClient, MockMesClient>();
        services.AddSingleton<IAlarmRepository, InMemoryAlarmRepository>();
        services.AddSingleton<ILoggerService, LoggerService>();

        services.AddSingleton<MachineController>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
