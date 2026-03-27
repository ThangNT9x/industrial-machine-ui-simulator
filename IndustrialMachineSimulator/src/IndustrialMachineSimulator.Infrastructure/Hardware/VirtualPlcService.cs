using IndustrialMachineSimulator.Core.Interfaces;

namespace IndustrialMachineSimulator.Infrastructure.Hardware;

public class VirtualPlcService : IPlcService
{
    public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public async Task MoveToAsync(double x, double y, CancellationToken cancellationToken = default)
    {
        await Task.Delay(500, cancellationToken);
    }

    public Task DisconnectAsync()
    {
        return Task.CompletedTask;
    }
}