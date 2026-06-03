using IndustrialMachineSimulator.Core.Entities;
using IndustrialMachineSimulator.Core.Interfaces;

namespace IndustrialMachineSimulator.Infrastructure.Networking;

public class MockMesClient : IMesClient
{
    public MesConnectionState ConnectionState { get; private set; } = MesConnectionState.Disconnected;

    public async Task ConnectAsync()
    {
        if (ConnectionState != MesConnectionState.Disconnected)
            return;

        ConnectionState = MesConnectionState.Connecting;
        await Task.Delay(800);
        ConnectionState = MesConnectionState.Connected;
    }

    public Task DisconnectAsync()
    {
        ConnectionState = MesConnectionState.Disconnected;
        return Task.CompletedTask;
    }

    public Task SendAsync(string messageType, string payload)
    {
        return Task.CompletedTask;
    }
}