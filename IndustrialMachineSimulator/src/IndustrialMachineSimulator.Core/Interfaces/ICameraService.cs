namespace IndustrialMachineSimulator.Core.Interfaces;

public interface ICameraService
{
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    Task<byte[]?> CaptureAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
}