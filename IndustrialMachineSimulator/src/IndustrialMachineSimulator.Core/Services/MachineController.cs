using IndustrialMachineSimulator.Core.Entities;
using IndustrialMachineSimulator.Core.Interfaces;

namespace IndustrialMachineSimulator.Core.Services;

public class MachineController
{
    private readonly ICameraService _cameraService;
    private readonly IPlcService _plcService;
    private readonly IMesClient _mesClient;
    private readonly ILoggerService _loggerService;

    public MachineStatusSnapshot Status { get; } = new();

    public MachineController(
        ICameraService cameraService,
        IPlcService plcService,
        IMesClient mesClient,
        ILoggerService loggerService)
    {
        _cameraService = cameraService;
        _plcService = plcService;
        _mesClient = mesClient;
        _loggerService = loggerService;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Status.State = MachineState.Initializing;
        _loggerService.Info("Initializing system...");

        Status.CameraConnected = await _cameraService.ConnectAsync(cancellationToken);
        Status.PlcConnected = await _plcService.ConnectAsync(cancellationToken);
        Status.MesConnected = await _mesClient.ConnectAsync(cancellationToken);

        Status.State = MachineState.Ready;
        _loggerService.Info("System ready.");
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        Status.State = MachineState.Running;
        _loggerService.Info("Machine started.");

        await _mesClient.SendHeartbeatAsync(cancellationToken);
    }

    public Task StopAsync()
    {
        Status.State = MachineState.Stopped;
        _loggerService.Info("Machine stopped.");
        return Task.CompletedTask;
    }
}