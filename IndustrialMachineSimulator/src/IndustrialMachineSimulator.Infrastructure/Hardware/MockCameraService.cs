using IndustrialMachineSimulator.Core.Interfaces;

namespace IndustrialMachineSimulator.Infrastructure.Hardware;

public class MockCameraService : ICameraService
{
    public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task<byte[]?> CaptureAsync(CancellationToken cancellationToken = default)
    {
        byte[] fakeImage = new byte[100];
        return Task.FromResult<byte[]?>(fakeImage);
    }

    public Task DisconnectAsync()
    {
        return Task.CompletedTask;
    }
}