namespace IndustrialMachineSimulator.Core.Interfaces;

public interface IMesClient
{
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    Task SendHeartbeatAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
}