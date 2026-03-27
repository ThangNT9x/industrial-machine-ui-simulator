namespace IndustrialMachineSimulator.Core.Interfaces;

public interface IPlcService
{
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    Task MoveToAsync(double x, double y, CancellationToken cancellationToken = default);
    Task DisconnectAsync();
}